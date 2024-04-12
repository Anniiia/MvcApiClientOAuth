﻿using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using MvcApiClientOAuth.Models;

namespace MvcApiClientOAuth.Services
{
    public class ServiceApiEmpleados
    {
        private string UrlApiEmpleados;
        private MediaTypeWithQualityHeaderValue Header;
        //objeto para recuperar HttpContexto y el User y su Clain
        private IHttpContextAccessor httpContextAccesor;

        public ServiceApiEmpleados(IConfiguration configuration, IHttpContextAccessor httpContextAccesor)
        {
            this.httpContextAccesor = httpContextAccesor;
            this.UrlApiEmpleados = configuration.GetValue<string>("ApiUrls:ApiEmpleados");
            this.Header = new MediaTypeWithQualityHeaderValue("application/json");
        }

        public async Task<string> GetTokenAsync(string username, string password)
        {

            using (HttpClient client = new HttpClient())
            {
                string request = "api/auth/login";
                client.BaseAddress = new Uri(this.UrlApiEmpleados);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);

                LoginModel model = new LoginModel
                {
                    UserName = username,
                    Password = password
                };

                string jsonData = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(jsonData, Encoding.UTF8,"application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    JObject keys = JObject.Parse(data);
                    string token = keys.GetValue("response").ToString();
                    return token;
                }
                else
                {
                    return null;
                }
            }
        }

        


        private async Task<T> CallApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApiEmpleados);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                HttpResponseMessage response = await client.GetAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;

                }
                else
                {
                    return default(T);
                }
            }

        }

        //tendremos un metodo generico que recibira el request y el token
        private async Task<T> CallApiAsync<T>(string request, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApiEmpleados);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization","bearer "+token);
                HttpResponseMessage response = await client.GetAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;

                }
                else
                {
                    return default(T);
                }
            }

        }

        public async Task<List<Empleado>> GetEmpleadosAsync()
        {
            string request = "api/empleados";
            List<Empleado> empleados = await this.CallApiAsync<List<Empleado>>(request);

            return empleados;
        }

        //metodo protegido

        public async Task<Empleado> FindEmpleadoAsync(int idEmpleado)
        {
            string request = "api/empleados/" + idEmpleado;
            Empleado empleado = await this.CallApiAsync<Empleado>(request);

            return empleado;

        }

        public async Task<Empleado> GetPerfilEmpleadoAsync()
        {
            string token = this.httpContextAccesor.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;

            string request = "api/empleados/perfilempleado";

            Empleado empleado = await this.CallApiAsync<Empleado>(request, token);

            return empleado;
        }

        public async Task<List<Empleado>> GetCompisTrabajoAsync()
        {
            string token = this.httpContextAccesor.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;

            string request = "api/empleados/compiscurro";

            List<Empleado> compis = await this.CallApiAsync<List<Empleado>>(request, token);

            return compis;
        }

        public async Task<List<string>> GetOficiosAsync()
        {
            string request = "api/empleados/oficios";

            List<string> oficios = await this.CallApiAsync<List<string>>(request);
            return oficios;
        }

        //esto podria ir en un helper

        private string TransformCollectionToQuery(List<string> collection)
        { 
            string result = "";

            foreach (string elem in collection)
            {
                result += "oficio=" + elem + "&";
            }

            result = result.TrimEnd('&');

            return result;
        }

        public async Task<List<Empleado>> GetEmpleadosOficiosAsync(List<string> oficios)
        {
            string request = "api/empleados/empleadosoficio";
            string data = this.TransformCollectionToQuery(oficios);
            List<Empleado> empleados = await this.CallApiAsync<List<Empleado>>(request + "?" + data);
            return empleados;
        }


        public async Task UpdateEmpleadosOficiosAsync(int incremento,List<string> oficio)
        {
            string request = "api/Empleados/incrementarsalariooficios/"+incremento;
            string data = this.TransformCollectionToQuery(oficio);
            List<Empleado> empleados = await this.CallApiAsync<List<Empleado>>(request + "?" + data);

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApiEmpleados);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                HttpResponseMessage response = await client.PutAsync(request + "?" + data, null);
            }
        }
    }
}
