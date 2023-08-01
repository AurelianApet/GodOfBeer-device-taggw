using device.util;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;

namespace device.restful
{
    public class ApiClient : GenericSingleton<ApiClient>
    {
        public class ApiInfo
        {
            public string api { get; set; }
            public object resObject { get; set; }
        }
        public class ApiResponse
        {
            public int? suc { get; set; }
            public int? status { get; set; }
            public string msg { get; set; }
            public Dictionary<string, object> dataMap { get; set; }
        }

        Dictionary<Type, ApiInfo> matchDic = null;

        JsonSerializer json = new JsonSerializer();

        public class TagVerifyApi
        {
            public int? tagGW_no { get; set; }
            public int? ch_value { get; set; }
            public string tag_data { get; set; }
        }

        public ApiClient()
        {
            matchDic = new Dictionary<Type, ApiInfo>();
            matchDic.Add(typeof(TagVerifyApi), new ApiInfo() { api = "tag-verify", resObject = new ApiResponse() });
        }

        public ApiResponse PostQuery(object postData)
        {
            ApiResponse result = null;
            try
            {
                var client = new RestClient(ConfigSetting.api_server_domain);
                var request = new RestRequest(ConfigSetting.api_prefix + matchDic[postData.GetType()].api, Method.POST);
                request.AddHeader("Content-Type", "application/json; charset=utf-8");
                request.AddJsonBody(postData);
                var response = client.Execute(request);
                result = json.Deserialize<ApiResponse>(response);
            }
            catch (Exception ex)
            {
                result = new ApiResponse();
                result.suc = 0;
                result.msg = ex.Message;
                result.dataMap = null;
            }
            return result;
        }

        public ApiResponse TagVerifyFunc(int ch_value, int tagGW_no, string tag_data)
        {
            TagVerifyApi info = new TagVerifyApi();
            info.tagGW_no = tagGW_no;
            info.ch_value = ch_value;
            info.tag_data = tag_data;
            return PostQuery(info);
        }
    }
}
