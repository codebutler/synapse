
using System;
using System.Reflection;

namespace Qyoto
{	
	public static class QTextBlockExtensions
	{	
		public static QTextBlock.iterator Begin(this QTextBlock self) {
			SmokeInvocation interceptor = (SmokeInvocation)self.GetType().GetField("interceptor", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField).GetValue(self);
            return (QTextBlock.iterator) interceptor.Invoke("begin", "begin() const", typeof(QTextBlock.iterator));
        }
	}
	
	public static class QTextBlockIteratorExtensions
	{
		public static QTextBlock.iterator Next(this QTextBlock.iterator self) {
			SmokeInvocation staticInterceptor = (SmokeInvocation)self.GetType().GetField("staticInterceptor", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField).GetValue(null);
			var i =(QTextBlock.iterator) staticInterceptor.Invoke("operator++", "operator++()", typeof(QTextBlock.iterator), typeof(QTextBlock.iterator), self);
			return i;
		}
	}
}
