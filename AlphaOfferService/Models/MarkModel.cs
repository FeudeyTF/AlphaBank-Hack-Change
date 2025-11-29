using AlphaOfferService.AlphaStructure.Clients;
using AlphaOfferService.Core;
using Microsoft.ML.OnnxRuntime;

namespace AlphaOfferService.Models
{
    internal class MarkModel : IIncomeModel, IDisposable
    {
        private readonly InferenceSession _session;

        public MarkModel(string modelPath)
        {
            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException($"ONNX модель по пути '{modelPath}' не найдена!");
            }

            _session = new InferenceSession(modelPath);
        }

        public async Task<int> CalculateClientIncome(IClient client)
        {
            return 0;
        }

        public void Dispose()
        {
            _session?.Dispose();
        }
    }
}
