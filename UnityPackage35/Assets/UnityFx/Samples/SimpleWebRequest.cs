using System;
using UnityEngine;
using UnityFx.Async;

/// <summary>
/// A simple wrapper over standard UnityWebRequest.
/// </summary>
public class SimpleWebRequest : AsyncResult<string>
{
	private WWW _www;

	public SimpleWebRequest(string url)
		: base(null)
	{
		_www = new WWW(url);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (_www != null)
		{
			if (_www.isDone)
			{
				if (string.IsNullOrEmpty(_www.error))
				{
					SetResult(_www.text);
				}
				else
				{
					// Can either throw or call SetException(...) to indacate operation failure.
					throw new Exception(_www.error);
				}

				_www.Dispose();
				_www = null;
			}
			else
			{
				SetProgress(_www.progress);
			}
		}
	}
}
