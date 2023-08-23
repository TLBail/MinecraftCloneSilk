using System.Reflection;
using System.Transactions;
using MinecraftCloneSilk.Logger.ChromeEvents;

namespace MinecraftCloneSilk.Logger;

using MethodBoundaryAspect.Fody.Attributes;

public sealed class Timer : OnMethodBoundaryAspect
{
   public override void OnEntry(MethodExecutionArgs args)
   {
      ChromeTrace.BeginTrace($"{args.Method.DeclaringType}.{args.Method.Name}");
   }

   public override void OnExit(MethodExecutionArgs args)
   {
      ChromeTrace.EndTrace($"{args.Method.DeclaringType}.{args.Method.Name}");
   }

}