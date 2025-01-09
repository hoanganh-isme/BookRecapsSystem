namespace Services.Responses
{
    public class TranscriptResponse
    {
        public TranscriptResponse(bool succeeded = true, string transcriptUrl = null, string errorMessage = null)
        {
            Succeeded = succeeded;
            TranscriptUrl = transcriptUrl;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Xác định xem yêu cầu gọi API có thành công hay không.
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// URL của transcript nếu gọi thành công.
        /// </summary>
        public string TranscriptUrl { get; set; }

        /// <summary>
        /// Thông báo lỗi nếu có.
        /// </summary>
        public string ErrorMessage { get; set; }
    }

}
