using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using To_Do_List_MVC_Refresher.Controllers;
using To_Do_List_MVC_Refresher.Models;

namespace To_Do_List_MVC_Refresher.Controllers
{
    public class ToDoListController : Controller
    {

        private IHttpClientFactory _client;

        public ToDoListController(IHttpClientFactory client)
        {
            _client = client;
        }

        //async because it needs to wait for the API's response over the network
        public async Task<IActionResult> ToDoListHomePage() //'Task' comes from the async method type (TB to async and sync lesson)
        {
            //Http client allows you to send a reques to to a url
            HttpClient client = _client.CreateClient(); //creates usable instance of a client
            client.BaseAddress = new Uri("https://localhost:7008"); //address at which your API is running 
            var response = await client.GetAsync("api/Task/getAll"); //Using the endpoint that gets the tasks you query at that address (this is the message object)
            var json = await response.Content.ReadAsStringAsync(); //C# sees the response as a plain JSON string 

            //converts the json intro c# object inorder to be read and shown in c# application
            var tasks = JsonSerializer.Deserialize<List<TaskItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(tasks);
        }

        //Consumer of the create endpoint
        [HttpPost]
        public async Task<IActionResult> AddTask(string title)
        {
            HttpClient client = _client.CreateClient(); //creates usable instance of a client
            client.BaseAddress = new Uri("https://localhost:7008");

            //create the object with the user enterd title and the status
            var newTask = new TaskItem { Title = title, IsComplete = false };

            //serialize the c# content 
            var content = new StringContent(JsonSerializer.Serialize(newTask), Encoding.UTF8, "application/json");

            //sending the serialized content using the endpoint for create
             await client.PostAsync("api/Task/create", content);

            return RedirectToAction("ToDoListHomePage");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            HttpClient client = _client.CreateClient(); //creates usable instance of a client
            client.BaseAddress = new Uri("https://localhost:7008");
            await client.DeleteAsync($"api/Task/delete/{id}");
            return RedirectToAction("ToDoListHomePage");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleComplete(Guid id)
        {
            HttpClient client = _client.CreateClient(); //creates usable instance of a client
            client.BaseAddress = new Uri("https://localhost:7008");

            //get a specific task using the endpoint that gets a specific task
            var getResponse = await client.GetAsync($"api/Task/get/{id}");//form of bytes

            //form of json 
            var json = await getResponse.Content.ReadAsStringAsync();

            //c# readable - converted from json to c#
            var task = JsonSerializer.Deserialize<TaskItem>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            task?.IsComplete = !task.IsComplete;//change status from false to true

            //send back the changed result in serialized version (so it could be read by the API)
            var content = new StringContent(JsonSerializer.Serialize(task?.IsComplete), Encoding.UTF8, "application/json");
            await client.PutAsync($"api/Task/update/{id}", content);

            return RedirectToAction("ToDoListHomePage");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

//private readonly HttpClient client;

//public ToDoListController()
//{
//    client = new HttpClient
//    {
//        BaseAddress = new Uri("https://localhost:7008")
//    };
//}