using Microsoft.ML.Data;

namespace ML
{
    public class ProductSalesData
    {
        [LoadColumn(0)]
        public string key_as_string;

        [LoadColumn(1)]
        public float doc_count;
    }
}