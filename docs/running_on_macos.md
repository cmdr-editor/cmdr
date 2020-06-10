## How can I run CMDR in my macOS?

There is three ways to edit your mappings in macOS. A fourth way is unconfirmed, but it has a lot of potential.

* [a) Xtrememapping](#a-simplest-xtrememapping)
* [b) Azure Cloud](#b-azure-cloud)
* [c) VirtualBox](#c-most-complex-virtualbox)
* [d) Winebottler](#d-potentially-very-simple-winebottler-emulation)


### a) Simplest: Xtrememapping

The simplest answer is: buy a copy of [XtremmeMapping](https://www.xtrememapping.com/) for macOS.

### b) Azure Cloud

The more complex answer is: get a free trial of a windows virtual machine. 

This will run in the "azure" microsoft cloud, and requires giving your credit card to detail to microsoft 
(they say over and over that no automatic billing will ever happen after the trial period without you changing your )

installation:
* link1: https://microsoft.github.io/AzureTipsAndTricks/blog/tip246.html
* link2: https://www.techrepublic.com/article/build-your-own-vm-in-the-cloud-with-microsoft-azure/

Then connect to your cloud machine using RDP for mac:
https://www.techrepublic.com/article/pro-tip-remote-desktop-on-mac-what-you-need-to-know/

Then:
  * Install CMDR as explained here: [CMDR installation instructions](https://github.com/pestrela/cmdr#download-and-installation)
  * Copy your TSI into the virtual machine (simplest is to use eg google drive on the browser)
 
### c) Most complex: VirtualBox

The most complex answer is: get a personal virtual machine couples with an evaluation copy of any Windows OS.

Step by step instructions are on:
  * https://towardsdatascience.com/how-to-install-a-free-windows-virtual-machine-on-your-mac-bf7cbc05888e
  * Step 1: Download VirtualBox
  * Step 2: Grab Windows 10
  * Step 3: Install VirtualBox and the extension pack
  * Step 4: Get your OS up and running

Then:
  * Install CMDR as explained here: [CMDR installation instructions](https://github.com/pestrela/cmdr#download-and-installation)
  * Copy your TSI into the virtual machine (simplest is to use eg google drive on the browser)
 
### d) Potentially very simple: Winebottler emulation

[Winebottler](https://winebottler.kronenberg.org/) is an emulation software that can run Windows-based programs directly on your Mac.\
This is possible thanks to a Windows-compatible subsystem provided by the very well-known linux tool called [Wine](https://www.winehq.org/)

https://winebottler.kronenberg.org/

Potentially this option could be the simoplest, but it cannot be tested until a mac user joins the project.\
So please share your experiences [here](https://github.com/cmdr-editor/cmdr/issues/1)

