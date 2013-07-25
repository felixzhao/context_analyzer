using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ICTCLASLib
{
    enum eWordRootType
    {
        NATURE_N , // n. 
        NATURE_V,  // v. 
        NATURE_ADJ, // adj
        NATURE_ADV, // adv
        NATURE_PRON, // pron
        NATURE_PREP, //prep
        NATURE_CONJ, // conj
        NATURE_T,    // time
        NATURE_W,    // punctuation
        NATURE_F,     // 方位词
        NATURE_M,	//	数词 数语素
        NATURE_Q 	//	量词 量语素
    }
    class SegWord
    {
        public string szWord;
        public string wordType; 
        public int Number; 
    }

    //////////////////////////////////////////////////////////////////////////
    // character coding types 
    //////////////////////////////////////////////////////////////////////////
    enum eCodeType
    {
        CODE_TYPE_UNKNOWN,//type unknown
        CODE_TYPE_ASCII,//ASCII
        CODE_TYPE_GB,//GB2312,GBK,GB10380
        CODE_TYPE_UTF8,//UTF-8
        CODE_TYPE_BIG5//BIG5
    }


    [StructLayout(LayoutKind.Sequential, Pack =1)]
    public struct result_t
    {
        [MarshalAs(UnmanagedType.U4)]
        public int start;
        [MarshalAs(UnmanagedType.U4)]
        public int length;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst =4)]
        public byte[] szPos;
        [MarshalAs(UnmanagedType.U4)]
        public int sPosLow;
        [MarshalAs(UnmanagedType.U4)]
        public int POS_id;
        [MarshalAs(UnmanagedType.U4)]
        public int word_ID;
        [MarshalAs(UnmanagedType.U4)]
        public int word_type;
        [MarshalAs(UnmanagedType.U4)]
        public int weight;

    }

    class ICTCLAS
    {
        public static bool initialize()
        {
            if (!ICTCLAS_Init(null))
                return false;
            ICTCLAS_ImportUserDict("userdic.txt", eCodeType.CODE_TYPE_UNKNOWN);
            return true; 
        }

        public static void splitword(string src, ArrayList wordList )
        {
            char[] chars = src.ToCharArray();
            result_t[] result = new result_t[src.Length];
            int i = 0;

            int nWrdCnt = ICTCLAS_ParagraphProcessAW(src, result, eCodeType.CODE_TYPE_UNKNOWN, 1 );
            result_t r;
            //取字符串真实长度:
            byte[] mybyte = System.Text.Encoding.Default.GetBytes(src);
            byte[] byteWord;
            for (i = 0; i < nWrdCnt; i++)
            {
                r = result[i];
                //String sWhichDic = "";

                //switch (r.word_type)
                //{
                //    case 0:
                //        sWhichDic = "核心词典";
                //        break;
                //    case 1:
                //        sWhichDic = "用户词典";
                //        break;
                //    case 2:
                //        sWhichDic = "专业词典";
                //        break;
                //    default:
                //        break;
                //}

                byteWord = new byte[r.length];
                //取字符串一部分
                Array.Copy(mybyte, r.start, byteWord, 0, r.length);
                SegWord segWord = new SegWord();
                segWord.szWord = System.Text.Encoding.Default.GetString(byteWord);
                segWord.wordType = System.Text.Encoding.Default.GetString(r.szPos).Trim('\0');
                segWord.Number = i ; 
                wordList.Add(segWord);
                //Console.WriteLine("No.{0}:start:{1}, length:{2},word_type:{3},Word_ID:{4}, UserDefine:{5}, Word:{6}\n", i, r.start, r.length, wordType, r.word_ID, sWhichDic, wrd);
            }

        }

        public static void uninitialize()
        {
             ICTCLAS_Exit();
        }

            
        #region DLL
        [DllImport("ICTCLAS50.dll", CharSet = CharSet.Ansi, EntryPoint = "ICTCLAS_Init",CallingConvention = CallingConvention.Cdecl)]
        private static extern bool ICTCLAS_Init(String sInitDirPath);

        [DllImport("ICTCLAS50.dll", CharSet = CharSet.Ansi, EntryPoint = "ICTCLAS_Exit", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool ICTCLAS_Exit();

        [DllImport("ICTCLAS50.dll", CharSet = CharSet.Ansi, EntryPoint = "ICTCLAS_ParagraphProcessAW", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ICTCLAS_ParagraphProcessAW(String sParagraph, [Out, MarshalAs(UnmanagedType.LPArray)]result_t[] result, eCodeType eCT, int bPOSTagged );

        [DllImport("ICTCLAS50.dll", CharSet = CharSet.Ansi, EntryPoint = "ICTCLAS_ImportUserDict", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ICTCLAS_ImportUserDict(String sFilename, eCodeType eCT);

        [DllImport("ICTCLAS50.dll", CharSet = CharSet.Ansi, EntryPoint = "ICTCLAS_FileProcess", CallingConvention = CallingConvention.Cdecl)]
        private static extern double ICTCLAS_FileProcess(String sSrcFilename, eCodeType eCt, String sDsnFilename, int bPOStagged);
        #endregion
    }
}
