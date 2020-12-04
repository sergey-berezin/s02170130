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

        [HttpDelete]
        public void Delete()
        {
            using var db = new ApplicationContext();
            db.ClearDataBase();
        }
    }
}
