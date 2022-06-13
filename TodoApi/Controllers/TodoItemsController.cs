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
        private List<Dictionary<string, string>> furnitureNamesAndSurface;
        private Dictionary<string, byte[]> photos;
        private Dictionary<string, byte[]> unity3d;
        private byte[] allPhotosInZip;
        private string path = ""; // enter the path to the photo folder here

        public TodoItemsController()
        {
            furnitureNamesAndSurface = new List<Dictionary<string, string>>();
            photos = new Dictionary<string, byte[]>();
            unity3d = new Dictionary<string, byte[]>();

            getNames();
            convertFilesToBytes("*png", photos);
            convertFilesToBytes("*unity3d", unity3d);
            packPhotosIntoZip();

        }

        [Route("start")]
        public async Task<ActionResult<string>> GetFurnitureNames()
        {
            var jsonName = JsonConvert.SerializeObject(furnitureNamesAndSurface);
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
            var newName = ti.ToTitleCase(name) + ".png";
            await Response.Body.WriteAsync(photos[newName], 0, photos[newName].Length);
        }

        [Route("asset/{name}")]
        public async Task ConvertUnity3dToBytes(string name)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            var newName = ti.ToTitleCase(name) + ".unity3d";
            if (!unity3d.Keys.Contains(newName))
                return;
            await Response.Body.WriteAsync(unity3d[newName]);
        }

        private void getNames()
        {
            var directories = Directory.GetDirectories(path);
            for (int i = 0; i < directories.Count(); i++)
            {
                var nameAndSurface = Path.GetFileName(directories[i]).Split("; ");
                var dict = new Dictionary<string, string>();
                dict[nameAndSurface[0]] = nameAndSurface[1];
                furnitureNamesAndSurface.Add(dict);
            }
        }

        private void convertFilesToBytes(string fileFormat, Dictionary<string, byte[]> bytes)
        {
            var directories = Directory.GetDirectories(path);
            foreach (var dir in directories)
            {
                var filePath = Directory.GetFiles(dir, fileFormat);
                if (filePath.Length == 0)
                    continue;
                var fileName = Path.GetFileName(filePath.First());
                bytes.Add(fileName, System.IO.File.ReadAllBytes(filePath.First()));
            }
        }

        private void packPhotosIntoZip()
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