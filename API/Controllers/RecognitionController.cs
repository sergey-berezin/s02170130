using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OnnxClassifier;
using static API.DataBaseUtils;


namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecognitionController : ControllerBase
    {
        [HttpPost]
        public ResultClassification Post([FromBody] ImageString obj)
        {
            ResultClassification result;
            OnnxClassifier.OnnxClassifier onnxModel;

            using (var db = new ApplicationContext())
            {
                result = db.FindInDataBase(obj);
                if (result != null)
                {
                    return result;
                }
                onnxModel = new OnnxClassifier.OnnxClassifier();
                result = onnxModel.PredictModel(obj.path);

                obj.path = result._PathImage;
                obj.Probability = result._Probability;
                obj.ClassImage = result._ClassImage;
                db.AddToDataBase(obj);
                

            }
            return result;


        }


        [HttpGet]
        public IEnumerable<string> Get()
        {
            using (var db = new ApplicationContext())
            {
                return db.Statistics();
            }

        }

        [HttpGet("AllInBase")]
        public IEnumerable<ImageString> GetAll()
        {
            using (var db = new ApplicationContext())
            {
                return db.GetAllImages();
            }

        }

        [HttpGet("{page}")]
        public IEnumerable<ImageString> Get(int page)
        {
            List<ImageString> result = new List<ImageString>();
            using (var db = new ApplicationContext())
            {
                List<ImageString> AllImagesInBd = db.GetAllImages();

                for (int i = page * 10; i < Math.Min(10 * (page + 1), AllImagesInBd.Count); i++)
                {
                    result.Add(AllImagesInBd[i]);
                }

                return result;
            }


            


        }

        [HttpDelete]
        public void Delete()
        {
            using var db = new ApplicationContext();
            db.ClearDataBase();
        }
    }
}
