using System;
using System.Collections;
using System.IO;

namespace Server
{
    public class TileMatrix
    {
        private Tile[][][][][] m_StaticTiles;
        private Tile[][][] m_LandTiles;

        private Tile[] m_InvalidLandBlock;
        private Tile[][][] m_EmptyStaticBlock;

        private FileStream m_Map;

        private FileStream m_Index;
        private BinaryReader m_IndexReader;

        private FileStream m_Statics;

        private int m_FileIndex;
        private int m_BlockWidth, m_BlockHeight;
        private int m_Width, m_Height;

        private Map m_Owner;

        private TileMatrixPatch m_Patch;
        private int[][] m_StaticPatches;
        private int[][] m_LandPatches;

#if !MONO
        [System.Runtime.InteropServices.DllImport("Kernel32")]
        private unsafe static extern int _lread(IntPtr hFile, void* lpBuffer, int wBytes);
#endif

        public Map Owner
        {
            get
            {
                return m_Owner;
            }
        }

        public TileMatrixPatch Patch
        {
            get
            {
                return m_Patch;
            }
        }

        public int BlockWidth
        {
            get
            {
                return m_BlockWidth;
            }
        }

        public int BlockHeight
        {
            get
            {
                return m_BlockHeight;
            }
        }

        public int Width
        {
            get
            {
                return m_Width;
            }
        }

        public int Height
        {
            get
            {
                return m_Height;
            }
        }

        public FileStream MapStream
        {
            get { return m_Map; }
            set { m_Map = value; }
        }

        public FileStream IndexStream
        {
            get { return m_Index; }
            set { m_Index = value; }
        }

        public FileStream DataStream
        {
            get { return m_Statics; }
            set { m_Statics = value; }
        }

        public BinaryReader IndexReader
        {
            get { return m_IndexReader; }
            set { m_IndexReader = value; }
        }

        public bool Exists
        {
            get { return (m_Map != null && m_Index != null && m_Statics != null); }
        }

        private static ArrayList m_Instances = new ArrayList();
        private ArrayList m_FileShare;

        public TileMatrix(Map owner, int fileIndex, int mapID, int width, int height)
        {
            m_FileShare = new ArrayList();

            for (int i = 0; i < m_Instances.Count; ++i)
            {
                TileMatrix tm = (TileMatrix)m_Instances[i];

                if (tm.m_FileIndex == fileIndex)
                {
                    tm.m_FileShare.Add(this);
                    m_FileShare.Add(tm);
                }
            }

            m_Instances.Add(this);
            m_FileIndex = fileIndex;
            m_Width = width;
            m_Height = height;
            m_BlockWidth = width >> 3;
            m_BlockHeight = height >> 3;

            m_Owner = owner;

            if (fileIndex != 0x7F)
            {
                string mapPath = Core.FindDataFile("map{0}.mul", fileIndex);

                if (File.Exists(mapPath))
                    m_Map = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                string indexPath = Core.FindDataFile("staidx{0}.mul", fileIndex);

                if (File.Exists(indexPath))
                {
                    m_Index = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    m_IndexReader = new BinaryReader(m_Index);
                }

                string staticsPath = Core.FindDataFile("statics{0}.mul", fileIndex);

                if (File.Exists(staticsPath))
                    m_Statics = new FileStream(staticsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }

            m_EmptyStaticBlock = new Tile[8][][];

            for (int i = 0; i < 8; ++i)
            {
                m_EmptyStaticBlock[i] = new Tile[8][];

                for (int j = 0; j < 8; ++j)
                    m_EmptyStaticBlock[i][j] = new Tile[0];
            }

            m_InvalidLandBlock = new Tile[196];

            m_LandTiles = new Tile[m_BlockWidth][][];
            m_StaticTiles = new Tile[m_BlockWidth][][][][];
            m_StaticPatches = new int[m_BlockWidth][];
            m_LandPatches = new int[m_BlockWidth][];

            m_Patch = new TileMatrixPatch(this, mapID);
        }

        public Tile[][][] EmptyStaticBlock
        {
            get
            {
                return m_EmptyStaticBlock;
            }
        }

        public void SetStaticBlock(int x, int y, Tile[][][] value)
        {
            if (x < 0 || y < 0 || x >= m_BlockWidth || y >= m_BlockHeight)
                return;

            if (m_StaticTiles[x] == null)
                m_StaticTiles[x] = new Tile[m_BlockHeight][][][];

            m_StaticTiles[x][y] = value;

            if (m_StaticPatches[x] == null)
                m_StaticPatches[x] = new int[(m_BlockHeight + 31) >> 5];

            m_StaticPatches[x][y >> 5] |= 1 << (y & 0x1F);
        }

        public Tile[][][] GetStaticBlock(int x, int y)
        {
            if (x < 0 || y < 0 || x >= m_BlockWidth || y >= m_BlockHeight || m_Statics == null || m_Index == null)
                return m_EmptyStaticBlock;

            if (m_StaticTiles[x] == null)
                m_StaticTiles[x] = new Tile[m_BlockHeight][][][];

            Tile[][][] tiles = m_StaticTiles[x][y];

            if (tiles == null)
            {
                for (int i = 0; tiles == null && i < m_FileShare.Count; ++i)
                {
                    TileMatrix shared = (TileMatrix)m_FileShare[i];

                    if (x >= 0 && x < shared.m_BlockWidth && y >= 0 && y < shared.m_BlockHeight)
                    {
                        Tile[][][][] theirTiles = shared.m_StaticTiles[x];

                        if (theirTiles != null)
                            tiles = theirTiles[y];

                        if (tiles != null)
                        {
                            int[] theirBits = shared.m_StaticPatches[x];

                            if (theirBits != null && (theirBits[y >> 5] & (1 << (y & 0x1F))) != 0)
                                tiles = null;
                        }
                    }
                }

                if (tiles == null)
                    tiles = ReadStaticBlock(x, y);

                m_StaticTiles[x][y] = tiles;
            }

            return tiles;
        }

        public Tile[] GetStaticTiles(int x, int y)
        {
            Tile[][][] tiles = GetStaticBlock(x >> 3, y >> 3);

            return tiles[x & 0x7][y & 0x7];
        }

        private static TileList m_TilesList = new TileList();

        public Tile[] GetStaticTiles(int x, int y, bool multis)
        {
            Tile[][][] tiles = GetStaticBlock(x >> 3, y >> 3);

            if (multis)
            {
                IPooledEnumerable eable = m_Owner.GetMultiTilesAt(x, y);

                if (eable == Map.NullEnumerable.Instance)
                    return tiles[x & 0x7][y & 0x7];

                bool any = false;

                foreach (Tile[] multiTiles in eable)
                {
                    if (!any)
                        any = true;

                    m_TilesList.AddRange(multiTiles);
                }

                eable.Free();

                if (!any)
                    return tiles[x & 0x7][y & 0x7];

                m_TilesList.AddRange(tiles[x & 0x7][y & 0x7]);

                return m_TilesList.ToArray();
            }
            else
            {
                return tiles[x & 0x7][y & 0x7];
            }
        }

        public void SetLandBlock(int x, int y, Tile[] value)
        {
            if (x < 0 || y < 0 || x >= m_BlockWidth || y >= m_BlockHeight)
                return;

            if (m_LandTiles[x] == null)
                m_LandTiles[x] = new Tile[m_BlockHeight][];

            m_LandTiles[x][y] = value;

            if (m_LandPatches[x] == null)
                m_LandPatches[x] = new int[(m_BlockHeight + 31) >> 5];

            m_LandPatches[x][y >> 5] |= 1 << (y & 0x1F);
        }

        public Tile[] GetLandBlock(int x, int y)
        {
            if (x < 0 || y < 0 || x >= m_BlockWidth || y >= m_BlockHeight || m_Map == null)
                return m_InvalidLandBlock;

            if (m_LandTiles[x] == null)
                m_LandTiles[x] = new Tile[m_BlockHeight][];

            Tile[] tiles = m_LandTiles[x][y];

            if (tiles == null)
            {
                for (int i = 0; tiles == null && i < m_FileShare.Count; ++i)
                {
                    TileMatrix shared = (TileMatrix)m_FileShare[i];

                    if (x >= 0 && x < shared.m_BlockWidth && y >= 0 && y < shared.m_BlockHeight)
                    {
                        Tile[][] theirTiles = shared.m_LandTiles[x];

                        if (theirTiles != null)
                            tiles = theirTiles[y];

                        if (tiles != null)
                        {
                            int[] theirBits = shared.m_LandPatches[x];

                            if (theirBits != null && (theirBits[y >> 5] & (1 << (y & 0x1F))) != 0)
                                tiles = null;
                        }
                    }
                }

                if (tiles == null)
                    tiles = ReadLandBlock(x, y);

                m_LandTiles[x][y] = tiles;
            }

            return tiles;
        }

        public Tile GetLandTile(int x, int y)
        {
            Tile[] tiles = GetLandBlock(x >> 3, y >> 3);

            return tiles[((y & 0x7) << 3) + (x & 0x7)];
        }

        private static TileList[][] m_Lists;

#if MONO
		private static byte[] m_Buffer;
#endif

        private static StaticTile[] m_TileBuffer = new StaticTile[128];

        private unsafe Tile[][][] ReadStaticBlock(int x, int y)
        {
            try
            {
                m_IndexReader.BaseStream.Seek(((x * m_BlockHeight) + y) * 12, SeekOrigin.Begin);

                int lookup = m_IndexReader.ReadInt32();
                int length = m_IndexReader.ReadInt32();

                if (lookup < 0 || length <= 0)
                {
                    return m_EmptyStaticBlock;
                }
                else
                {
                    int count = length / 7;

                    m_Statics.Seek(lookup, SeekOrigin.Begin);

                    if (m_TileBuffer.Length < count)
                        m_TileBuffer = new StaticTile[count];

                    StaticTile[] staTiles = m_TileBuffer;//new StaticTile[tileCount];

                    fixed (StaticTile* pTiles = staTiles)
                    {
#if !MONO
                        _lread(m_Statics.Handle, pTiles, length);
#else
						if ( m_Buffer == null || length > m_Buffer.Length )
							m_Buffer = new byte[length];

						m_Statics.Read( m_Buffer, 0, length );

						fixed ( byte *pbBuffer = m_Buffer )
						{
							StaticTile *pCopyBuffer = (StaticTile *)pbBuffer;
							StaticTile *pCopyEnd = pCopyBuffer + count;
							StaticTile *pCopyCur = pTiles;

							while ( pCopyBuffer < pCopyEnd )
								*pCopyCur++ = *pCopyBuffer++;
						}
#endif

                        if (m_Lists == null)
                        {
                            m_Lists = new TileList[8][];

                            for (int i = 0; i < 8; ++i)
                            {
                                m_Lists[i] = new TileList[8];

                                for (int j = 0; j < 8; ++j)
                                    m_Lists[i][j] = new TileList();
                            }
                        }

                        TileList[][] lists = m_Lists;

                        StaticTile* pCur = pTiles, pEnd = pTiles + count;

                        while (pCur < pEnd)
                        {
                            lists[pCur->m_X & 0x7][pCur->m_Y & 0x7].Add((short)((pCur->m_ID & 0x3FFF) + 0x4000), pCur->m_Z);
                            ++pCur;
                        }

                        Tile[][][] tiles = new Tile[8][][];

                        for (int i = 0; i < 8; ++i)
                        {
                            tiles[i] = new Tile[8][];

                            for (int j = 0; j < 8; ++j)
                                tiles[i][j] = lists[i][j].ToArray();
                        }

                        return tiles;
                    }
                }
            }
            catch (EndOfStreamException)
            {
                if (DateTime.Now >= m_NextStaticWarning)
                {
                    Console.WriteLine("Warning: Static EOS for {0} ({1}, {2})", m_Owner, x, y);
                    m_NextStaticWarning = DateTime.Now + TimeSpan.FromMinutes(1.0);
                }

                return m_EmptyStaticBlock;
            }
        }

        private DateTime m_NextStaticWarning;
        private DateTime m_NextLandWarning;

        public void Force()
        {
            if (ScriptCompiler.Assemblies == null || ScriptCompiler.Assemblies.Length == 0)
                throw new Exception();
        }

        private unsafe Tile[] ReadLandBlock(int x, int y)
        {
            try
            {
                m_Map.Seek(((x * m_BlockHeight) + y) * 196 + 4, SeekOrigin.Begin);

                Tile[] tiles = new Tile[64];

                fixed (Tile* pTiles = tiles)
                {
#if !MONO
                    _lread(m_Map.Handle, pTiles, 192);
#else
					if ( m_Buffer == null || 192 > m_Buffer.Length )
						m_Buffer = new byte[192];

					m_Map.Read( m_Buffer, 0, 192 );

					fixed ( byte *pbBuffer = m_Buffer )
					{
						Tile *pBuffer = (Tile *)pbBuffer;
						Tile *pEnd = pBuffer + 64;
						Tile *pCur = pTiles;

						while ( pBuffer < pEnd )
							*pCur++ = *pBuffer++;
					}
#endif
                }

                return tiles;
            }
            catch
            {
                if (DateTime.Now >= m_NextLandWarning)
                {
                    Console.WriteLine("Warning: Land EOS for {0} ({1}, {2})", m_Owner, x, y);
                    m_NextLandWarning = DateTime.Now + TimeSpan.FromMinutes(1.0);
                }

                return m_InvalidLandBlock;
            }
        }

        public void Dispose()
        {
            if (m_Map != null)
                m_Map.Close();

            if (m_Statics != null)
                m_Statics.Close();

            if (m_IndexReader != null)
                m_IndexReader.Close();
        }
    }

    public class TileMatrixPatch
    {
        private int m_LandBlocks, m_StaticBlocks;

        private static bool m_Enabled = true;

        public static bool Enabled
        {
            get
            {
                return m_Enabled;
            }
            set
            {
                m_Enabled = value;
            }
        }

#if !MONO
        [System.Runtime.InteropServices.DllImport("Kernel32")]
        private unsafe static extern int _lread(IntPtr hFile, void* lpBuffer, int wBytes);
#endif

        public int LandBlocks
        {
            get
            {
                return m_LandBlocks;
            }
        }

        public int StaticBlocks
        {
            get
            {
                return m_StaticBlocks;
            }
        }

        public TileMatrixPatch(TileMatrix matrix, int index)
        {
            if (!m_Enabled)
                return;

            string mapDataPath = Core.FindDataFile("mapdif{0}.mul", index);
            string mapIndexPath = Core.FindDataFile("mapdifl{0}.mul", index);

            if (File.Exists(mapDataPath) && File.Exists(mapIndexPath))
                m_LandBlocks = PatchLand(matrix, mapDataPath, mapIndexPath);

            string staDataPath = Core.FindDataFile("stadif{0}.mul", index);
            string staIndexPath = Core.FindDataFile("stadifl{0}.mul", index);
            string staLookupPath = Core.FindDataFile("stadifi{0}.mul", index);

            if (File.Exists(staDataPath) && File.Exists(staIndexPath) && File.Exists(staLookupPath))
                m_StaticBlocks = PatchStatics(matrix, staDataPath, staIndexPath, staLookupPath);
        }

        private unsafe int PatchLand(TileMatrix matrix, string dataPath, string indexPath)
        {
            using (FileStream fsData = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (FileStream fsIndex = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryReader indexReader = new BinaryReader(fsIndex);

                    int count = (int)(indexReader.BaseStream.Length / 4);

                    for (int i = 0; i < count; ++i)
                    {
                        int blockID = indexReader.ReadInt32();
                        int x = blockID / matrix.BlockHeight;
                        int y = blockID % matrix.BlockHeight;

                        fsData.Seek(4, SeekOrigin.Current);

                        Tile[] tiles = new Tile[64];

                        fixed (Tile* pTiles = tiles)
                        {
#if !MONO
                            _lread(fsData.Handle, pTiles, 192);
#else
							if ( m_Buffer == null || 192 > m_Buffer.Length )
								m_Buffer = new byte[192];

							fsData.Read( m_Buffer, 0, 192 );

							fixed ( byte *pbBuffer = m_Buffer )
							{
								Tile *pBuffer = (Tile *)pbBuffer;
								Tile *pEnd = pBuffer + 64;
								Tile *pCur = pTiles;

								while ( pBuffer < pEnd )
									*pCur++ = *pBuffer++;
							}
#endif
                        }

                        matrix.SetLandBlock(x, y, tiles);
                    }

                    indexReader.Close();

                    return count;
                }
            }
        }

#if MONO
		private static byte[] m_Buffer;
#endif

        private static StaticTile[] m_TileBuffer = new StaticTile[128];

        private unsafe int PatchStatics(TileMatrix matrix, string dataPath, string indexPath, string lookupPath)
        {
            using (FileStream fsData = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (FileStream fsIndex = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (FileStream fsLookup = new FileStream(lookupPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        BinaryReader indexReader = new BinaryReader(fsIndex);
                        BinaryReader lookupReader = new BinaryReader(fsLookup);

                        int count = (int)(indexReader.BaseStream.Length / 4);

                        TileList[][] lists = new TileList[8][];

                        for (int x = 0; x < 8; ++x)
                        {
                            lists[x] = new TileList[8];

                            for (int y = 0; y < 8; ++y)
                                lists[x][y] = new TileList();
                        }

                        for (int i = 0; i < count; ++i)
                        {
                            int blockID = indexReader.ReadInt32();
                            int blockX = blockID / matrix.BlockHeight;
                            int blockY = blockID % matrix.BlockHeight;

                            int offset = lookupReader.ReadInt32();
                            int length = lookupReader.ReadInt32();
                            lookupReader.ReadInt32(); // Extra

                            if (offset < 0 || length <= 0)
                            {
                                matrix.SetStaticBlock(blockX, blockY, matrix.EmptyStaticBlock);
                                continue;
                            }

                            fsData.Seek(offset, SeekOrigin.Begin);

                            int tileCount = length / 7;

                            if (m_TileBuffer.Length < tileCount)
                                m_TileBuffer = new StaticTile[tileCount];

                            StaticTile[] staTiles = m_TileBuffer;//new StaticTile[tileCount];

                            fixed (StaticTile* pTiles = staTiles)
                            {
#if !MONO
                                _lread(fsData.Handle, pTiles, length);
#else
								if ( m_Buffer == null || length > m_Buffer.Length )
									m_Buffer = new byte[length];

								fsData.Read( m_Buffer, 0, length );

								fixed ( byte *pbBuffer = m_Buffer )
								{
									StaticTile *pCopyBuffer = (StaticTile *)pbBuffer;
									StaticTile *pCopyEnd = pCopyBuffer + tileCount;
									StaticTile *pCopyCur = pTiles;

									while ( pCopyBuffer < pCopyEnd )
										*pCopyCur++ = *pCopyBuffer++;
								}
#endif

                                StaticTile* pCur = pTiles, pEnd = pTiles + tileCount;

                                while (pCur < pEnd)
                                {
                                    lists[pCur->m_X & 0x7][pCur->m_Y & 0x7].Add((short)((pCur->m_ID & 0x3FFF) + 0x4000), pCur->m_Z);
                                    ++pCur;
                                }

                                Tile[][][] tiles = new Tile[8][][];

                                for (int x = 0; x < 8; ++x)
                                {
                                    tiles[x] = new Tile[8][];

                                    for (int y = 0; y < 8; ++y)
                                        tiles[x][y] = lists[x][y].ToArray();
                                }

                                matrix.SetStaticBlock(blockX, blockY, tiles);
                            }
                        }

                        indexReader.Close();
                        lookupReader.Close();

                        return count;
                    }
                }
            }
        }
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct Tile
    {
        internal short m_ID;
        internal sbyte m_Z;

        public int ID
        {
            get
            {
                return m_ID;
            }
        }

        public int Z
        {
            get
            {
                return m_Z;
            }
            set
            {
                m_Z = (sbyte)value;
            }
        }

        public int Height
        {
            get
            {
                if (m_ID < 0x4000)
                {
                    return 0;
                }
                else
                {
                    return TileData.ItemTable[m_ID & 0x3FFF].Height;
                }
            }
        }

        public bool Ignored
        {
            get
            {
                return (m_ID == 2 || m_ID == 0x1DB || (m_ID >= 0x1AE && m_ID <= 0x1B5));
            }
        }

        public Tile(short id, sbyte z)
        {
            m_ID = id;
            m_Z = z;
        }

        public void Set(short id, sbyte z)
        {
            m_ID = id;
            m_Z = z;
        }
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct StaticTile
    {
        public short m_ID;
        public byte m_X;
        public byte m_Y;
        public sbyte m_Z;
        public short m_Hue;
    }
}