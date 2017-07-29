using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityFx.Async;

/// <summary>
/// A simple wrapper over standard UnityWebRequest.
/// </summary>
public class SimpleWebRequest : AsyncResult<string>
{
	private UnityWebRequest _www;

	public SimpleWebRequest(string url)
		: base(null)
	{
		_www = UnityWebRequest.Get(url);
		_www.Send();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (_www != null)
		{
			if (_www.isDone)
			{
				if (_www.isNetworkError || _www.isHttpError)
				{
					// Can either throw or call SetException(...) to indacate operation failure.
					throw new Exception(_www.error);
				}
				else
				{
					SetResult(_www.downloadHandler.text);
				}

				_www.Dispose();
				_www = null;
			}
			else
			{
				SetProgress(_www.downloadProgress);
			}
		}
	}
}
