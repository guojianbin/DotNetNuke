using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace DotNetNuke.Services.ClientCapability
{
    public class ChtmlUi
    {
        public KnownDevice Device { get; set; }
        public string ImodeRegion { get { return Device.Capability<string>("imode_region"); } }
        public string MakePhoneCallString { get { return Device.Capability<string>("chtml_make_phone_call_string"); } }
        public bool CanDisplayImagesAndTextOnSameLine { get { return Device.Capability<bool>("chtml_can_display_images_and_text_on_same_line"); } }
        public bool DisplaysImageInCenter { get { return Device.Capability<bool>("chtml_displays_image_in_center"); } }
        public bool TableSupport { get { return Device.Capability<bool>("chtml_table_support"); } }
        public bool DisplayAccesskey { get { return Device.Capability<bool>("chtml_display_accesskey"); } }
        public bool Emoji { get { return Device.Capability<bool>("emoji"); } }

    }
}