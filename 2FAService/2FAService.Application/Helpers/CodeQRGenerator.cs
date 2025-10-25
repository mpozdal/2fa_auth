using QRCoder;

namespace TwoFactorService.Application.Helpers
{
    public static class CodeQRGenerator
    {
        public static string GenerateQrCodeAsBase64(string qrCodeData)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeInfo = qrGenerator.CreateQrCode(qrCodeData, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeInfo);
            byte[] qrCodeImageBytes = qrCode.GetGraphic(20);

            return "data:image/png;base64," + Convert.ToBase64String(qrCodeImageBytes);
        }
    }
}
