using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GerenteAPI
{
    /// <summary>
    /// Class utilizada para realizar requisiçoes para APIs
    /// </summary>
    public class ClienteAPI
    {
        public TimeSpan Timeout { get; set; }
        public static T DeserializeObject<T>(string json) => JsonConvert.DeserializeObject<T>(json);
        public static string SerializeObject(object obj) => JsonConvert.SerializeObject(obj);


        /// <summary>
        /// Configuração padrão para os verbos
        /// </summary>
        /// <param name="client"></param>
        /// <param name="get"></param>
        /// <param name="mediaType"></param>
        private void ConfigPadrao(HttpClient client, bool get = false, string mediaType = "")
        {
            client.Timeout = Timeout == null ? new TimeSpan(0, 1, 0) : Timeout;
            if (get)
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
            }
        }

        public  T Delete<T>(string urlAPi, string json, string mediaType = "application/json")
        {
            using (var client = new HttpClient(new HttpClientHandler()) { BaseAddress = new Uri(urlAPi) })
            {
                ConfigPadrao(client);
                HttpContent content = new StringContent(json, UTF8Encoding.UTF8, mediaType);
                HttpResponseMessage response = client.DeleteAsync(client.BaseAddress).Result;
                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
        }

        /// <summary>
        /// Realiza requisições PUT
        /// </summary>
        /// <typeparam name="T">Entidade retornada pelo endpoint</typeparam>
        /// <param name="urlAPi">URL da requisição</param>
        /// <param name="json">Dados da requisição, serão passados no corpo da requisição como json</param>
        /// <param name="mediaType"> Tipo de dados, padrão 'application/json'</param>
        /// <returns>Objeto do tipo T, desserealizado da API</returns>
        public  T Put<T>(string urlAPi, string json, string urlJson = "", string mediaType = "application/json", Dictionary<string, string> defaultHeaders = null)
        {
            try
            {
                using (var client = new HttpClient(new HttpClientHandler()) { BaseAddress = new Uri(urlAPi + urlJson) })
                {
                    ConfigPadrao(client);
                    SetDefaultHeader(client, defaultHeaders);
                    HttpContent content = new StringContent(json, UTF8Encoding.UTF8, mediaType);
                    HttpResponseMessage response = client.PutAsync(client.BaseAddress, content).Result;
                    return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (ArgumentNullException erro) { throw erro; } //Solicitação Nula
            catch (InvalidOperationException erro) { throw erro; } //Mensagem de solicitação já foi enviada pela instância HttpClient.
            catch (HttpRequestException erro) { throw erro; } //A solicitação falhou devido a um problema subjacente, como conectividade de rede, falha de DNS, validação de certificado de servidor ou tempo limite.
            catch (TaskCanceledException erro) { throw erro; }//Solicitação expirou ou usuario cancelou
            catch (Exception erro) { throw erro; }//outros erros
        }



        /// <summary>
        /// Realizar Get 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="urlAPi">url api</param>
        /// <param name="json">json</param>
        /// <param name="urlJson">utl do json</param>
        /// <param name="mediaType">tipo de media</param>
        /// <returns>objeto do tipo T</returns>
        public  T Get<T>(string urlAPi, string json, string urlJson = "", string mediaType = "application/json")
        {
            try
            {

                using (var client = new HttpClient() { BaseAddress = new Uri(urlAPi + urlJson) })
                {

                    ConfigPadrao(client, true, mediaType);
                    HttpResponseMessage response = client.GetAsync(json).Result;
                    return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (ArgumentNullException erro) { throw erro; } //Solicitação Nula
            catch (InvalidOperationException erro) { throw erro; } //Mensagem de solicitação já foi enviada pela instância HttpClient.
            catch (HttpRequestException erro) { throw erro; } //A solicitação falhou devido a um problema subjacente, como conectividade de rede, falha de DNS, validação de certificado de servidor ou tempo limite.
            catch (TaskCanceledException erro) { throw erro; }//Solicitação expirou ou usuario cancelou
            catch (Exception erro) { throw erro; }//outros erros

        }

        /// <summary>
        /// Realizar download de algo
        /// </summary>
        /// <param name="data">url</param>
        /// <returns>conteudo baixado especificado em uma string</returns>
        public  string DownloadData(string data)
        {
            using (var client = new WebClient())
            {
                return client.DownloadString(data);
                // return client.DownloadData(data);
            }
        }


        /// <summary>
        /// Realiza o post usando HttpRequestMessage paro o tipo {T} e tipo {K}
        /// </summary>
        /// <typeparam name="T">Tipo de primeira tentativa de serilização</typeparam>
        /// <typeparam name="K">Tipo de segunda tentativa de serilização</typeparam>
        /// <param name="urlAPi"></param>
        /// <param name="json"></param>
        /// <param name="mediaType"></param>
        /// <param name="defaultHeaders"></param>
        /// <returns></returns>
        public  Tuple<T, K> PostMessage<T, K>(string urlAPi, string json,
            string mediaType = "application/json",
            Dictionary<string, string> defaultHeaders = null,
            string authorization = ""
            )
            where K : class
            where T : class

        {
            var request = new HttpRequestMessage(HttpMethod.Post, urlAPi);

            if (!string.IsNullOrEmpty(authorization))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(authorization);
            }
            //Headrs
            foreach (var item in defaultHeaders)
            {
                request.Headers.Add(item.Key, item.Value);
            }

            request.Content = new StringContent(json, Encoding.UTF8, mediaType);

            var response = new HttpClient().SendAsync(request).Result;
            var t = response.Content.ReadAsStringAsync().Result;

            //tentativa tipo T
            try
            {
                return new Tuple<T, K>(JsonConvert.DeserializeObject<T>(t), null);
            }
            catch (Exception erro)
            {
                return new Tuple<T, K>(null, JsonConvert.DeserializeObject<K>(t));
            }

        }


        /// <summary>
        /// Realiza o post usando HttpClient padrão
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="urlAPi"></param>
        /// <param name="json"></param>
        /// <param name="mediaType"></param>
        /// <param name="defaultHeaders"></param>
        /// <returns></returns>
        public  T Post<T>(string urlAPi, string json,
            string mediaType = "application/json",
            Dictionary<string, string> defaultHeaders = null
            )
        {
            try
            {
                using (var client = new HttpClient(new HttpClientHandler()) { BaseAddress = new Uri(urlAPi) })
                {
                    ConfigPadrao(client);
                    SetDefaultHeader(client, defaultHeaders);
                    HttpContent content = new StringContent(json, UTF8Encoding.UTF8, mediaType);
                    HttpResponseMessage response = client.PostAsync(client.BaseAddress, content).Result;
                    Task<string> task = response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(task.Result);
                }
            }
            catch (ArgumentNullException erro) { throw erro; } //Solicitação Nula
            catch (InvalidOperationException erro) { throw erro; } //Mensagem de solicitação já foi enviada pela instância HttpClient.
            catch (HttpRequestException erro) { throw erro; } //A solicitação falhou devido a um problema subjacente, como conectividade de rede, falha de DNS, validação de certificado de servidor ou tempo limite.
            catch (TaskCanceledException erro) { throw erro; }//Solicitação expirou ou usuario cancelou
            catch (Exception erro) { throw erro; }//outros erros
        }

        private  void SetDefaultHeader(in HttpClient client, in Dictionary<string, string> headers)
        {
            if (headers is object && headers.Count > 0)
            {
                foreach (KeyValuePair<string, string> item in headers)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }
        }
    }
}
