package crc6494337cccf7d1a104;


public class MainActivity_GameKeyListener
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.view.View.OnKeyListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onKey:(Landroid/view/View;ILandroid/view/KeyEvent;)Z:GetOnKey_Landroid_view_View_ILandroid_view_KeyEvent_Handler:Android.Views.View/IOnKeyListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("SnapClone.Android.MainActivity+GameKeyListener, SnapClone.Android", MainActivity_GameKeyListener.class, __md_methods);
	}

	public MainActivity_GameKeyListener ()
	{
		super ();
		if (getClass () == MainActivity_GameKeyListener.class) {
			mono.android.TypeManager.Activate ("SnapClone.Android.MainActivity+GameKeyListener, SnapClone.Android", "", this, new java.lang.Object[] {  });
		}
	}

	public MainActivity_GameKeyListener (crc6494337cccf7d1a104.MainActivity p0)
	{
		super ();
		if (getClass () == MainActivity_GameKeyListener.class) {
			mono.android.TypeManager.Activate ("SnapClone.Android.MainActivity+GameKeyListener, SnapClone.Android", "SnapClone.Android.MainActivity, SnapClone.Android", this, new java.lang.Object[] { p0 });
		}
	}

	public boolean onKey (android.view.View p0, int p1, android.view.KeyEvent p2)
	{
		return n_onKey (p0, p1, p2);
	}

	private native boolean n_onKey (android.view.View p0, int p1, android.view.KeyEvent p2);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
