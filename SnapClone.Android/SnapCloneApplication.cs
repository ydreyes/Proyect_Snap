using System;
using Android.App;
using Android.Runtime;

namespace SnapClone.Android
{
    [Application(Label = "SnapClone", Icon = "@drawable/snapclone_icon", Theme = "@style/SnapCloneTheme")]
    public class SnapCloneApplication : Application
    {
        public SnapCloneApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }
    }
}
