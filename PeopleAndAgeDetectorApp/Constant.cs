namespace AgeDetectorApp
{
    public static class Constant
    {
        public const string CAFFE_MODEL_URL = "https://drive.google.com/file/d/1eka0fAlz20YOSJrzuzIb0URay5bSzxaq/view";
        public const string CAFFE_MODEL = "TrainedModels/age_net.caffemodel";
        public const string DEPLOY_AGE = "TrainedModels/deploy_age.prototxt";
        public const string HAARCASCADE_FRONTALFACE_DEFAULT = "TrainedModels/haarcascade_frontalface_default.xml";
        public static readonly string[] AGES = 
        [
         "0-3", "4-7", "8-14", "15-24", "25-37", "38-47", "48-53", "60-100"
        ];
    }
}
