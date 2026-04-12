using Application.DTOs;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Auth
{
    public class VerifyCaptchaCommand : IRequest<VerifyCaptchaResponseDTO>
    {
        public string CaptchaResponse { get; set; }
    }

    public class VerifyCaptchaCommandHandler : IRequestHandler<VerifyCaptchaCommand, VerifyCaptchaResponseDTO>
    {
        private const string GoogleRecaptchaVerifyUrl = "https://www.google.com/recaptcha/api/siteverify";
        private const string SecretKey = "6Ld55_MqAAAAAGp5FMQBEdh3ssw26uzRqdAUWwRb"; 

        public VerifyCaptchaCommandHandler()
        {
        }

        public async Task<VerifyCaptchaResponseDTO> Handle(VerifyCaptchaCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.CaptchaResponse))
            {
                return new VerifyCaptchaResponseDTO
                {
                    Success = false,
                    ErrorMessage = "Captcha response is required."
                };
            }

            var url = $"{GoogleRecaptchaVerifyUrl}?secret={SecretKey}&response={request.CaptchaResponse}";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(url, null, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Deserialize using Newtonsoft.Json
                var result = JsonConvert.DeserializeObject<GoogleCaptchaResponse>(responseContent);

                if (result?.Success == true)
                {
                    return new VerifyCaptchaResponseDTO
                    {
                        Success = true,
                        Message = "Captcha verified successfully."
                    };
                }

                return new VerifyCaptchaResponseDTO
                {
                    Success = false,
                    ErrorMessage = result?.ErrorCodes != null
                        ? string.Join(", ", result.ErrorCodes)
                        : "Captcha verification failed."
                };
            }
        }
    }

    public class GoogleCaptchaResponse
    {
        public bool Success { get; set; }
        public string[] ErrorCodes { get; set; }
    }
}
