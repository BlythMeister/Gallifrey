---
title: Windows 
description: TODO
---
# SmartScreen Help

[Windows SmartScreen](https://en.wikipedia.org/wiki/Microsoft_SmartScreen) in Windows 8 onwards seems to be causing users some bother.
In the interim (until we have our own Microsoft approved certificate or enough users for Microsoft to trust us) the following steps will help!

If you have the ability to update your organisations group policy, you could add "http://releases.gallifreyapp.co.uk" & "https://releases.gallifreyapp.co.uk" into your "Intranet Zone".
By doing this you will circumvent the Windows SmartScreen feature as this does not apply to intranet zone applications.

When prompted with this:

![Smartscreen page 1]({{ site.url }}/images/help/smartscreen1.png "OK Screen")

Press the "More info" text

When prompted with this:

![Smartscreen page 2]({{ site.url }}/images/help/smartscreen2.png "Run anyway Screen")

Press the "Run anyway" text

If you had Gallifrey pinned, you will need to un-pin & re-pin.
