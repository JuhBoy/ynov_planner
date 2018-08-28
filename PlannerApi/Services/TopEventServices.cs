using System;
using events_planner.Models;

namespace Microsoft.Extensions.DependencyInjection {

  public class TopEventServices : ITopEventServices {

    private PlannerContext Context { get; set; }

    public TopEventServices(PlannerContext context) {
        Context = context;
    }

    public TopEvents[] ReArrangeTopEvents(TopEvents[] tops) {
        int cursor = 1;
        foreach(var top in tops) {
            top.Index = cursor;
            cursor++;
        }
        return tops;
    }
  }

}