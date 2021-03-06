<?php

//check if querystrings exist or not
if(empty($_GET['u']) || empty($_GET['e']) || empty($_GET['h']) ||
    empty($_GET['addr']) || empty($_GET['app']) || empty($_GET['name']))
{
    //no querystrings or wrong syntax
    echo "Unauthorized! 50101";
    header('HTTP/1.0 404 Not Found');
    exit(1);
}
else
{
    //querystring exists
    $username = $_GET['u'];
    $expires = $_GET['e'];
    $userhash = $_GET['h'];

    $app = $_GET['app'];
    $name = $_GET['name'];
    $addr = $_GET['addr'];
}

// Specify the secret key
$secret = "MySecretString";

// *****************************************
// Specify authorized users
$passwords = [
    ["ted",        "password1"],
    ["tod",        "password2"],
    ["tim",        "password3"]
];

$pass = "";
$count = count($passwords);

for($i = 0; $i < $count; $i++)
{
    if($passwords[$i][0] == $username)
    {
        $pass = $passwords[$i][1];
    }
}

// *****************************************

if ($pass == "")
{
    echo "Unauthorized!";
    header('HTTP/1.0 404 Not Found');
    exit(1);
}

// Check the md5 hash
$myhash = md5("$username$expires$app$addr$name$secret$pass");

if (strtolower($userhash) != strtolower($myhash))
{
    echo "Unauthorized!";
    header('HTTP/1.0 404 Not Found');
    exit(1);
}

// Check if hash is expired
if (time() > $expires)
{
    echo "Unauthorized!";
    header('HTTP/1.0 404 Not Found');
    exit(1);
}

echo "Greetings! ";
?>