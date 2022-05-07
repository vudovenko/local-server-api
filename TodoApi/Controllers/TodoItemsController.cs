#nullable disable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private string[] furnitureNames;
        private Dictionary<string, byte[]> photos;
        private byte[] allPhotosInZip;
        private string path = ""; // enter the path to the photo folder here

        public TodoItemsController()
        {
            photos = new Dictionary<string, byte[]>();

            GetNames();
            ConvertPhotosToBytes();
            PackPhotosIntoZip();

        }

        [Route("start")]
        public async Task<ActionResult<string>> GetFurnitureNames()
        {
            var jsonName = JsonConvert.SerializeObject(furnitureNames);
            return Content(jsonName);
        }

        [Route("catalog")]
        public async Task GetPhotos()
        {
            await Response.Body.WriteAsync(allPhotosInZip, 0, allPhotosInZip.Length);
        }

        [Route("catalog/{name}")]
        public async Task GetPhotos(string name)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            var newName = ti.ToTitleCase(name) + ".jpg";
            await Response.Body.WriteAsync(photos[newName], 0, photos[newName].Length);
        }

        //[Route("asset/{name}")]
        //public async Task GetPhotos(string name)
        //{

        //}

        private void GetNames()
        {
            var directories = Directory.GetDirectories(path);
            var names = new string[directories.Count()];
            for (int i = 0; i < directories.Count(); i++)
                names[i] = Path.GetFileName(directories[i]);
            furnitureNames = names;
        }        

        private void ConvertPhotosToBytes()
        {
            var directories = Directory.GetDirectories(path);
            //var names = new string[directories.Count()];
            foreach (var dir in directories)
            {
                var photoPath = Directory.GetFiles(dir, "*.jpg");
                var photoName = Path.GetFileName(photoPath.First());
                photos.Add(photoName, System.IO.File.ReadAllBytes(photoPath.First()));
            }
        }

        private void PackPhotosIntoZip()
        {
            using (var zipToOpen = new MemoryStream())
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    foreach (var photoName in photos.Keys)
                    {
                        ZipArchiveEntry readmeEntry = archive.CreateEntry(photoName);
                        using (Stream stream = readmeEntry.Open())
                        {
                            stream.Write(photos[photoName]);
                        }
                    }
                }
                allPhotosInZip = zipToOpen.ToArray();
            }
        }
    }
}