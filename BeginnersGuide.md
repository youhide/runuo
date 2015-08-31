# Step 1: TortoiseSVN #

Download and install TortoiseSVN from:
http://tortoisesvn.net/downloads.html

Make sure to install the 64-bits version if you are running on 64-bits Windows.


# Step 2: Downloading the RunUO code #

Create a new, empty folder for the code to be downloaded in. Right-click on the folder, select "SVN Checkout".

Fill in the URL of the repository:
http://runuo.googlecode.com/svn/devel/

Click OK to begin downloading.


# Step 3: Compiling the core #

After the code has finished downloading, run `compile_net_2.bat` to create the core. If you have .NET 4 fully installed you can use `compile_net_4.bat` instead.


# Step 4: Starting RunUO #

You can now run `RunUO.exe` to open your server. You can connect to your local server using IP address `127.0.0.1` and port `2593`.