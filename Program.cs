using CodeNation_Cript.Helper;
using CodeNation_Cript.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeNation_Cript
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {

            string data = "";

            // Get information wiht my token
            try
            {
                WebRequest request = WebRequest.Create("https://api.codenation.dev/v1/challenge/dev-ps/generate-data?token=5fe5322db44b1c301caa43551e71a0d495620f71");
                var response = request.GetResponse();
                using (Stream dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                    data = reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
            }

            // Parsing
            var resultado = JsonConvert.DeserializeObject<Answer>(data);

            // Decrypt
            resultado.decifrado = DecryptJulioCesar(1, resultado.cifrado);

            // SHA1
            resultado.resumo_criptografico = HashSha1.Hash(resultado.decifrado);

            // Show data
            Console.WriteLine("Cifrado = {0}", resultado.cifrado);
            Console.WriteLine("Decifrado = {0}", resultado.decifrado);
            Console.WriteLine("Decifrado = {0}", resultado.resumo_criptografico);


            // Generate temp file
            string filePath = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".json";
            File.WriteAllText(filePath, JsonConvert.SerializeObject(resultado));

            // Post data with multipart/form-data
            using (var httpClient = new HttpClient())
            using (var form = new MultipartFormDataContent())
            using (var fs = File.OpenRead(filePath))
            using (var streamContent = new StreamContent(fs))
            using (var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync()))
            {
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                form.Add(fileContent, "answer", Path.GetFileName(filePath));
                HttpResponseMessage response = await httpClient.PostAsync("https://api.codenation.dev/v1/challenge/dev-ps/submit-solution?token=5fe5322db44b1c301caa43551e71a0d495620f71", form);
                data = await response.Content.ReadAsStringAsync();
            }

            Console.WriteLine("Post Result = {0}", data);
            Console.ReadKey();
        }

        static string DecryptJulioCesar(int numeroCasas, string textoCriptografado)
        {
            string textoDescriptografado = "";

            // Varrendo todos caracteres da string
            foreach (var item in textoCriptografado.ToLower())
            {
                // verificando se é um caracter válido
                if ((int)item >= 97 && (int)item<=122)
                { 
                    textoDescriptografado += (char)((int)item+(numeroCasas*-1));
                }
                else
                {
                    textoDescriptografado += item;
                }
            }

            return textoDescriptografado;
        }
    }
}
