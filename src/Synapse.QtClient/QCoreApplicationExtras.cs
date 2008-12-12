namespace Qyoto {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;
	using System.Text;

	class EventReceiver : QObject {
		public EventReceiver(QObject parent) : base(parent) {}
		
		public override bool Event(QEvent e) {
			if (e.type() == QEvent.TypeOf.User) {
				ThreadEvent my = (ThreadEvent) e;
				my.dele();
				my.handle.Free();  // free the handle so the event can be collected
				return true;
			}
			return false;
		}
	}

	class ThreadEvent : QEvent {
		public NoArgDelegate dele;
		public GCHandle handle;
		
		public ThreadEvent(NoArgDelegate d) : base(QEvent.TypeOf.User) {
			dele = d;
		}
	}
}