using System;
using events_planner.Models;

namespace Microsoft.Extensions.DependencyInjection {

  public interface ITopEventServices {
      TopEvents[] ReArrangeTopEvents(TopEvents[] tops);
  }

}
