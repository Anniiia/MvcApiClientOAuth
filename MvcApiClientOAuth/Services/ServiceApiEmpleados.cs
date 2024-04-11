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

        public ServiceApiEmpleados(IConfiguration configuration)
        {
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
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
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
                    return "Peticion incorrecta " + response.StatusCode;
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

        public async Task<List<Empleado>> GetEmpleadoAsync()
        {
            string request = "aspi/empleados";
            List<Empleado> empleados = await this.CallApiAsync<List<Empleado>>(request);

            return empleados;
        }

        //metodo protegido

        public async Task<Empleado> FindEmpleadoAsync(int idEmpleado, string token)
        {
            string request = "aspi/empleados/" + idEmpleado;
            Empleado empleado = await this.CallApiAsync<Empleado>(request, token);

            return empleado;

        }

    }
}