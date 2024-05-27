using System.Text.Json;
using System.Text.Json.Serialization;

namespace GrpcDemoRESTService
{
    public class WeatherReply
    {
        public WeatherInfo Actual { get; set; }
        public Dictionary<int, WeatherInfo> Forecast { get; set; } = new Dictionary<int, WeatherInfo>();
    }

    public class WeatherInfo
    {
        public int Pressure { get; set; }
        public int Temperature { get; set; }
        public int WindSpeed {  get; set; }
        public WeatherStatus Status {  get; set; }
        public List<string> Warnning { get; set; } = new List<string>();
        private int? degree;

        [JsonIgnore]
        public int? Degree
        {
            get => degree;
            set
            {
                degree = value;
                if (value.HasValue)
                {
                    Code = null; // Reset the other value when this one is set
                }
            }
        }

        private WindDirection? code;

        [JsonIgnore]
        public WindDirection? Code
        {
            get => code;
            set
            {
                code = value;
                if (value.HasValue)
                {
                    Degree = null; // Reset the other value when this one is set
                }
            }
        }
        [JsonPropertyName("wind_direction")]
        public object WindDirection
        {
            get
            {
                if (Degree.HasValue)
                {
                    return Degree.Value;
                }
                if (Code.HasValue)
                {
                    return Code.Value.ToString();
                }
                return null;
            }
        }
    }

    public enum WindDirection
    {
        NONE = 0,
        N = 1,
        NE = 2,
        E = 3,
        SE = 4,
        S = 5,
        SW = 6,
        W = 7,
        NW = 8,
        VAR = 9,
    }

    public enum WeatherStatus
    {
        NONE = 0,
        CLEAR = 1,
        CLOUDS = 2,
        RAIN = 3,
        SNOW = 4,
        MIST = 5,
    }
}
