using System;

namespace ILib.Contents
{
	using Flag = Content.TransLockFlag;

	internal class TransLock
	{
		Flag m_Flag;
		public bool IsLock() => m_Flag != 0;
		public bool IsLock(Flag flag) => m_Flag.HasFlag(flag);

		public IDisposable Lock(Flag flag)
		{
			if (IsLock(Flag.Shutdown)) throw new InvalidOperationException("already Shutdown AppContext");
			if (IsLock(flag)) throw new InvalidOperationException("already locked:" + flag);
			return new LockState(this, flag);
		}

		public class LockState : IDisposable
		{
			TransLock m_transLock;
			Flag m_flag;
			public LockState(TransLock transLock, Flag flag)
			{
				m_transLock = transLock;
				m_flag = flag;
			}
			public void Dispose()
			{
				m_transLock.m_Flag = m_transLock.m_Flag & m_flag;
				m_transLock = null;
			}
		}

	}
}
