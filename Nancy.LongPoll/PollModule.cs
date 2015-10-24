﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading;
using Nancy;
using Newtonsoft.Json;

namespace Nancy.LongPoll
{
  public class PollModule : NancyModule
  {
    #region Nancy's module implementation
    ISessionProvider _SessionProvider = null;
    PollService _PollService = null;
    TinyIoc.TinyIoCContainer _Container = null;

    public PollModule(TinyIoc.TinyIoCContainer container, PollService pollService = null, ISessionProvider sessionProvider = null)
    {
      if (container == null) throw new ArgumentNullException("container");
      if (!(sessionProvider is DefaultSessionProvider)) _SessionProvider = sessionProvider;

      if (pollService == null)
      {
        container.Register<PollService>().AsSingleton();
        pollService = container.Resolve<PollService>();
      }
      _PollService = pollService;

      Get["/Poll/Register"] = x =>
      {
        var sp = _SessionProvider;
        if (sp == null) sp = new DefaultSessionProvider(Request);

        var response = Response.AsJson(_PollService.Register(Request.UserHostAddress, sp.SessionId));
        if (sp is DefaultSessionProvider)
        {
          response = response.WithCookie(DefaultSessionProvider.SessionIdCookieName, sp.SessionId);
        }

        return response;
      };
      Get["/Poll/Wait"] = x =>
      {
        string clientId = Request.Query.clientId;
        ulong seqCode = Request.Query.seqCode;

        return Response.AsJson(_PollService.Wait(clientId, seqCode));
      };
    }
    #endregion
  }
}
