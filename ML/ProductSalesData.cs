using Microsoft.ML.Data;

namespace ML
{
    public class ProductSalesData
    {
        [LoadColumn(0)]
        public string timestamp;

        [LoadColumn(1)]
        public float count;
    }
}