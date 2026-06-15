using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace MyWeb.Pages
{
    public class Test : PageModel
    {
        public string Days
        {
            get; set;
        }
        public string Meal1
        {
            get; set;
        }
        
        public async Task OnGet()
        {
            DateTime today = DateTime.Today;
            
            Days = DateTime.Today.ToString("yyyy-MM-dd dddd", new CultureInfo("ko-KR"));
            await GetMeal();
        }

        private async Task GetMeal()
        {
            DateTime time = DateTime.Today;
            
            string date = time.ToString("yyyyMMdd");
            var client = new HttpClient();
            string url = $"https://open.neis.go.kr/hub/mealServiceDietInfo?KEY=0f5025c0e64d44c899a35d416e33031a&Type=JSON&ATPT_OFCDC_SC_CODE=N10&SD_SCHUL_CODE=8140108&MLSV_YMD={date}";
            var response = await client.GetStringAsync(url);

            using JsonDocument doc = JsonDocument.Parse(response);

            // 급식 데이터 없을 때 대비
            if (!doc.RootElement.TryGetProperty("mealServiceDietInfo", out var meal))
            {
                Meal1 = "급식 정보 없음";
                return;
            }
            
            var mealData = doc.RootElement
            .GetProperty("mealServiceDietInfo")[1]
            .GetProperty("row")[0]
            .GetProperty("DDISH_NM")
            .GetString();

            Meal1 = mealData.Replace("<br/>", "\n");
        }

        public async Task<IActionResult> OnGetMeal(string date)
        {
            var client = new HttpClient();

            string url = $"https://open.neis.go.kr/hub/mealServiceDietInfo?KEY=0f5025c0e64d44c899a35d416e33031a&Type=JSON&ATPT_OFCDC_SC_CODE=N10&SD_SCHUL_CODE=8140108&MLSV_YMD={date}";

            var response = await client.GetStringAsync(url);

            using JsonDocument doc = JsonDocument.Parse(response);

            if (!doc.RootElement.TryGetProperty("mealServiceDietInfo", out var meal) 
            || meal.GetArrayLength() < 2)
            {
                return Content("급식 정보 없음");
            }

            var mealData = meal[1]
                .GetProperty("row")[0]
                .GetProperty("DDISH_NM")
                .GetString();

            return Content(mealData.Replace("<br/>", "\n"));
        }
    }
}