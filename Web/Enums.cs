using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPanther.Framework.Web
{
	/// <summary>
	/// Denotes what type of url-encoding is necessary for a target website.
	/// </summary>
	public enum UrlEncodingType
	{
		Normal = 1,
		MediaPanther = 2,
		LB = 3
	}
}