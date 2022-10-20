# **VNA Test Automation with DMX OpenTAP Plugin**

Keysight DMX (S94601B) API TAP Plugin - This plugin makes it possible for Keysight Test Automation and OpenTap users to automate creating measurements setups, performing measurement sweeps and collecting data using the Device Measurement eXpert application remote programming API.

Some keys functionalities provided by the plugin are:

- Establish connection with an instance of DmxApi
- Load a DMX configuration file (\*.dmx)
- Interrogate the list of channels and measurements in the loaded configuration
- Connect to an instance of a VNA instrument
- Build either all the channels in the loaded configuration or selected individual channels on the VNA instance.
- Sweep the built channels on the VNA instance
- Collect data from the channels on the VNA instance

The plugin is very useful for automating measurement setup and data acquisition for complex active devices on a Keysight Vector Network Analyzer.

**Prerequisites**

- [Keysight S94601B DMX version A.3.0.2.00](https://www.keysight.com/us/en/product/S94601B/device-measurement-expert-dmx.html?jmpid=zzfinddmx) or later installed on the same PC running the plugin.
- A valid license for S94601B
- A valid license for S94610B (if loaded configurations include Wideband Digital Transceiver measurements)
- A supported Keysight VNA or a supported VNA simulator
  - Benchtop VNAs:PNA/PNA-L/PNA-X models N52xxB with firmware version A.15.75.xx or newer. ENA E5080B model with firmware version A.15.75.xx or newer. Also Modular & USB VNAs: PXI M98xxA modular VNA models and Streamline Series USB VNA P50XXA models

**Getting OpenTAP**

If you are looking to use OpenTAP, you can get pre-built binaries at [opentap.io](https://opentap.io/).

Using the OpenTAP CLI you are now able to download plugin packages from the OpenTAP package repository.

To list and install plugin packages do the following in the command prompt:

tap package list

tap package install \<Package Name\>

We recommend that you download the Software Development Kit, or simply the Developer's System Community Edition provided by Keysight Technologies. The Developer System is a bundle that contain the SDK as well as a graphical user interface and result viewing capabilities. It can be installed by typing the following:

tap package install "Developer's System CE" -y

**Installing the plugin**

The plugin is available as a package on [OpenTAP package repository](https://packages.opentap.io/). To install, use the following command in an OpenTAP installation:

tap package install "VNA Test Automation with DMX" -r packages.opentap.io