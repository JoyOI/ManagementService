using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.MapperProfiles
{
    public class BaseMapperProfile : Profile
    {
        private readonly static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public BaseMapperProfile()
        {
            // 时间 <=> 字符串
            CreateMap<DateTime, string>().ConvertUsing(d => d.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss"));
            CreateMap<string, DateTime>().ConvertUsing(s => DateTime.Parse(s).ToUniversalTime());

            // 时间 <=> 时间戳
            CreateMap<DateTime, long>().ConvertUsing(d => (long)(d - Epoch).TotalSeconds);
            CreateMap<long, DateTime>().ConvertUsing(t => Epoch.AddSeconds(t));

            // 字符串 <=> Base64字符串
            CreateMap<string, byte[]>().ConvertUsing(s => Convert.FromBase64String(s));
            CreateMap<byte[], string>().ConvertUsing(b => Convert.ToBase64String(b));
        }
    }
}
