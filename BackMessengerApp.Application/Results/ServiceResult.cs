
namespace BackMessengerApp.Application.Results
{
	namespace Bot.Application.Results
	{
		public class ServiceResult<T> : BaseResult
		{
			public T Data { get; private set; }
			public bool IsSuccessful { get; private set; }
			public List<string> Errors { get; private set; } = new List<string>();


			public static ServiceResult<T> Success(T data)
			{
				return new ServiceResult<T>()
				{
					Data = data,
					Errors = new List<string>(),
					IsSuccessful = true
				};
			}
			public static ServiceResult<T> Success()
			{
				return new ServiceResult<T>()
				{
					Data = default,
					Errors = new List<string>(),
					IsSuccessful = true
				};
			}
			public static ServiceResult<T> Fail(T data, string error)
			{
				return new ServiceResult<T>()
				{
					Data = data,
					Errors = new List<string>() { error },
					IsSuccessful = false
				};
			}
			public static ServiceResult<T> Fail(string error)
			{
				return new ServiceResult<T>()
				{
					Data = default,
					Errors = new List<string>() { error },
					IsSuccessful = false
				};
			}
			public static ServiceResult<T> Fail(T data, List<string> errors)
			{
				return new ServiceResult<T>()
				{
					Data = data,
					Errors = errors,
					IsSuccessful = false
				};
			}
			public static ServiceResult<T> Fail(List<string> errors)
			{
				return new ServiceResult<T>()
				{
					Data = default,
					Errors = errors,
					IsSuccessful = false
				};
			}
		}
	}

}
