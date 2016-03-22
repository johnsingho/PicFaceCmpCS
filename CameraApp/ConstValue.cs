using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraApp
{
    class ConstValue
    {
        internal static readonly string DEF_SYS_NAME = "人脸识别自助进站系统";

        //提示音路径
        public const string VOICE_DIR = "voice\\";

        public const string VOICE_INIT_OK      = "InitOk.wav";
        public const string VOICE_INIT_FAIL    = "InitFail.wav";
        public const string VOICE_PASS         = "pass.wav";
        public const string VOICE_FAIL_FACECMP = "FaceCmpFail.wav";
        public const string VOICE_FAIL_TICKCHK = "TicketChkFail.wav";
        public const string VOICE_VIEW_CAM     = "viewcam.wav";
        public const string VOICE_PLACE_TIC    = "placeTicket.wav";

        /*
        
#define CLR_TITLE     RGB(255, 69, 0)
#define CLR_INFO      RGB(11, 0, 0)
#define CLR_ERROR     RGB(220, 20, 60)
#define CLR_SUCCESS   RGB(0,128, 0)
#define CLR_INFO_READ RGB(20, 100, 20)

//日志文件名
#define SYSTEMLOG_FILENAME  "system.log"
#define FACECMPLOG_FILENAME "FaceCmp.log"
*/


    }
}
