using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EDA.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Json;
using Newtonsoft.Json.Linq;
using IniParser;
using IniParser.Model;
using System.IO;
using System.Text;
using System.Text.Json;


namespace EDA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public List<Dados> lerArquivo() //verifica se o arquivo existe e está vazio, caso contrario chama a função que irá popular ele com os dados da API
        {
            FileIniDataParser file = new FileIniDataParser();

            List<Dados> dadosLidos = new List<Dados>();

            IniData read = file.ReadFile("governo.ini");

            foreach (var dt in read["API_GOVERNO"])
            {

                Dados novoDado = new Dados();
                novoDado.codigo = dt.KeyName;
                novoDado.descricao = dt.Value;
                dadosLidos.Add(novoDado);
            }

            return (dadosLidos);
        }

        public async Task<IActionResult> Index()
        {
            //ira verificar se existe algum arquivo de dados chamado governo.ini, caso não houver, irá criar um novo e chamar a Api para popular o mesmo
            List<Dados> govData = new List<Dados>();
            FileIniDataParser file = new FileIniDataParser();
            FileInfo fi = new FileInfo("governo.ini");
            if (!fi.Exists)
                fi.Create();
            else
             if (lerArquivo().Count > 0)
            {
                govData = lerArquivo();
                Console.WriteLine("leu arquivo");
            }
            else
            {
                //chama api e preenche o arquivo que estava vazio 
                using (var httpClient = new HttpClient())
                {
                    Console.WriteLine("buscou da api");
                    httpClient.DefaultRequestHeaders.Add("chave-api-dados", "25a3d8a82b88332c75724aa202037345");
                    using (var response = await httpClient.GetAsync("http://api.portaldatransparencia.gov.br/api-de-dados/orgaos-siafi?pagina=1"))

                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        govData = JsonConvert.DeserializeObject<List<Dados>>(apiResponse);

                        IniData read = file.ReadFile("governo.ini");
                        foreach (var informacao in govData)
                        {
                            read["API_GOVERNO"][informacao.codigo] = informacao.descricao;
                            file.WriteFile("governo.ini", read);

                        }
                    }
                }
            }
            return View(govData); //retorna pra view index
        }

        //método para retornar o último elemento da lista
        public string lastElement()
        {
            List<Dados> read = lerArquivo();
            List<Dados> retorno = new List<Dados>();
            Console.WriteLine("chegou no controller lastElement");
            for (int i = 0; i < read.Count; i++)
            {
                if (i + 1 == read.Count)
                {
                    retorno.Add(read[i]);
                    Console.WriteLine("chegou ao ultimo item: " + read[i].descricao);
                }

            }

            var json = JsonConvert.SerializeObject(retorno);
            Console.WriteLine(json.ToString());


            return (json.ToString());
        }

        public void inputBinario()
        {
            List<int> vetorList = new List<int>();

            foreach (Dados d in lerArquivo())
            {
                vetorList.Add(int.Parse(d.codigo));
            }
            int[] arr = vetorList.ToArray();

            int n = arr.Length;
            int x = 12000;
            int result = buscaBinaria(arr, x);
            if (result == -1)
                Console.WriteLine("Elemento nao encontrado");

            else
                Console.WriteLine("Element encontrado no "
                                  + "index " + result);


        }

        public int buscaBinaria(int[] arr, int x)
        {
            int l = 0, r = arr.Length - 1;
            while (l <= r)
            {
                int m = l + (r - l) / 2;

                // verifica se x está no meio
                if (arr[m] == x)
                    return m;

                //se x for maior ignora o lado esquerdo
                if (arr[m] < x)
                    l = m + 1;

                //se x for menor ignora o lado direito
                else
                    r = m - 1;
            }

            //caso nao encontrado
            return -1;
        }


        public void linear()
        {
            Console.WriteLine("chegou no controller 1");
            List<int> vetorList = new List<int>();

            foreach (Dados d in lerArquivo())
            {
                vetorList.Add(int.Parse(d.codigo));
            }
            int[] arr = vetorList.ToArray();
            int search_element = 12804;

            // chamada da funcao
            buscaLinear(arr, search_element);
        }

        public void buscaLinear(int[] arr, int search_Element)
        {
            int left = 0;
            int length = arr.Length;
            int right = length - 1;
            int position = -1;

            // começa o loop da esquerda para a direita
            for (left = 0; left <= right;)
            {

                // caso for encontrado no lado esquerdo
                if (arr[left] == search_Element)
                {
                    position = left;
                    Console.WriteLine(
                        "Elemento encontrado no lado esquerdo em "
                        + (position + 1) + " com  "
                        + (left + 1) + " tentativas");
                    break;
                }

                // caso encontrado no lado direito
                if (arr[right] == search_Element)
                {
                    position = right;
                    Console.WriteLine(
                        "Elemento encontrado no lado direito em "
                        + (position + 1) + " com "
                        + (length - right) + " tentativas");
                    break;
                }

                left++;
                right--;
            }

            // elemento nao encontrado
            if (position == -1)
                Console.WriteLine("Elemento nao encontrado");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
