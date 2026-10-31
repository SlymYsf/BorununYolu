using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BorununYolu.LLM
{
    /// <summary>
    /// Modelin ham metin yanıtını NpcResponse nesnesine çevirir ve doğrular.
    /// Bozuk JSON, eksik alan veya yanlış tip durumunda false döner; exception fırlatmaz.
    /// </summary>
    public static class NpcResponseParser
    {
        public const int MinOfke = 0;
        public const int MaxOfke = 10;
        private const int MaxCevapLength = 500;

        public static bool TryParse(string raw, out NpcResponse response, out string error)
        {
            response = null;
            error = null;

            if (string.IsNullOrWhiteSpace(raw))
            {
                error = "Yanıt boş.";
                return false;
            }

            string json = ExtractJsonObject(raw);
            if (json == null)
            {
                error = "Yanıtta JSON nesnesi bulunamadı.";
                return false;
            }

            JObject obj;
            try
            {
                obj = JObject.Parse(json);
            }
            catch (JsonException ex)
            {
                error = "JSON ayrıştırılamadı: " + ex.Message;
                return false;
            }

            // Zorunlu alanlar
            if (!TryGetString(obj, "cevap", out string cevap))
            {
                error = "'cevap' alanı eksik veya metin değil.";
                return false;
            }

            if (!TryGetBool(obj, "kapiyi_ac", out bool kapiyiAc))
            {
                error = "'kapiyi_ac' alanı eksik veya true/false değil.";
                return false;
            }

            if (!TryGetInt(obj, "ofke", out int ofke))
            {
                error = "'ofke' alanı eksik veya sayı değil.";
                return false;
            }

            // İsteğe bağlı alan: yoksa kavga başlamaz.
            if (!TryGetBool(obj, "kavga_baslat", out bool kavgaBaslat))
                kavgaBaslat = false;

            cevap = cevap.Trim();

            if (cevap.Length > MaxCevapLength)
                cevap = cevap.Substring(0, MaxCevapLength).TrimEnd() + "...";

            ofke = Math.Max(MinOfke, Math.Min(MaxOfke, ofke));

            response = new NpcResponse(cevap, kapiyiAc, kavgaBaslat, ofke);
            return true;
        }

        /// <summary>
        /// Model bazen JSON'u ```json ... ``` bloğu içinde ya da önüne/arkasına metin ekleyerek döndürür.
        /// İlk '{' ile son '}' arasını alarak asıl nesneyi çıkarır.
        /// </summary>
        private static string ExtractJsonObject(string raw)
        {
            int start = raw.IndexOf('{');
            int end = raw.LastIndexOf('}');
            if (start < 0 || end <= start) return null;
            return raw.Substring(start, end - start + 1);
        }

        private static bool TryGetString(JObject obj, string key, out string value)
        {
            value = null;
            var token = obj[key];
            if (token == null || token.Type != JTokenType.String) return false;
            value = token.Value<string>();
            return !string.IsNullOrWhiteSpace(value);
        }

        private static bool TryGetBool(JObject obj, string key, out bool value)
        {
            value = false;
            var token = obj[key];
            if (token == null) return false;

            if (token.Type == JTokenType.Boolean)
            {
                value = token.Value<bool>();
                return true;
            }

            // Küçük modeller bazen "true" / "false" metni döndürür; bunu da kabul et.
            if (token.Type == JTokenType.String && bool.TryParse(token.Value<string>(), out value))
                return true;

            return false;
        }

        private static bool TryGetInt(JObject obj, string key, out int value)
        {
            value = 0;
            var token = obj[key];
            if (token == null) return false;

            switch (token.Type)
            {
                case JTokenType.Integer:
                    value = token.Value<int>();
                    return true;
                case JTokenType.Float:
                    value = (int)Math.Round(token.Value<double>());
                    return true;
                case JTokenType.String:
                    return int.TryParse(token.Value<string>(), out value);
                default:
                    return false;
            }
        }
    }
}
