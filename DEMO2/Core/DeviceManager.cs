using DEMO2.Drivers;

namespace DEMO2.Core
{
    public class DeviceManager
    {
        private static readonly DeviceManager _instance = new DeviceManager();
        public static DeviceManager Instance => _instance;

        // 실제 드라이버 클래스를 직접 참조
        public DTP7HDriver TP { get; private set; }
        public MotionController Motion { get; private set; }

        private DeviceManager()
        {
            TP = new DTP7HDriver();
            Motion = new MotionController();
        }

        // 외부에서 간편하게 연결 상태를 확인할 수 있는 로직
        public bool IsReady => TP != null && TP.IsConnected;
    }
}