﻿using System.Net;

namespace RedMango_API.Models
{
	public class ApiResponse
	{
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; } = true;
        public List<string> ErrorList { get; set; } = new List<string>();
        public object Result { get; set; }
	}
}
