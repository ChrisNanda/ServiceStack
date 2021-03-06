﻿using System;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceHost;
using ServiceStack.Text;
using ServiceStack.Text.Common;

namespace ServiceStack.ServiceInterface
{
	public class SessionFactory : ISessionFactory
	{
		private readonly ICacheClient cacheClient;

		public SessionFactory(ICacheClient cacheClient)
		{
			this.cacheClient = cacheClient;
		}

		public class SessionCacheClient : ISession
		{
			private readonly ICacheClient cacheClient;
			private string prefixNs;

			public SessionCacheClient(ICacheClient cacheClient, string sessionId)
			{
				this.cacheClient = cacheClient;
				this.prefixNs = "sess:" + sessionId + ":";
			}

			public object this[string key]
			{
				get
				{
					return cacheClient.Get<object>(this.prefixNs + key);
				}
				set
				{
					JsWriter.WriteDynamic(() => 
						cacheClient.Set(this.prefixNs + key, value));
				}
			}

			public void Set<T>(string key, T value)
			{
				cacheClient.Set(this.prefixNs + key, value);
			}

			public T Get<T>(string key)
			{
				return cacheClient.Get<T>(this.prefixNs + key);
			}
		}

		public ISession GetOrCreateSession(IHttpRequest httpReq, IHttpResponse httpRes)
		{
			var sessionId = httpReq.GetCookieValue(SessionFeature.PermanentSessionId)
				?? httpRes.CreatePermanentSessionId(httpReq);

			return new SessionCacheClient(cacheClient, sessionId);
		}
	}
}