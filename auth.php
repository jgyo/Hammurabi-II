<?php
// Specify the shared secret key
$secret = "SixthKingOfTheFirstBabylonianDynasty";

// For 4.3.0 <= PHP <= 5.4.0
if (!function_exists('http_response_code'))
{
    function http_response_code($newcode = NULL)
    {
        static $code = 200;
        if($newcode !== NULL)
        {
            header('X-PHP-Response-Code: '.$newcode, true, $newcode);
            if(!headers_sent())
                $code = $newcode;
        }

        return $code;
    }
}

// Insure this is a local request
//if ($_SERVER['SERVER_ADDR'] != $_SERVER['REMOTE_ADDR'])
//{
//    http_response_code(403);
//    echo "<h1>403 Forbidden</h1>";
//    exit(1);
//}

require "includes/database.php";

//check if querystrings exist or not
if(empty($_POST['u']) || empty($_POST['e']) || empty($_POST['h']) ||
    empty($_POST['addr']) || empty($_POST['app']) || empty($_POST['name']))
{
    //no querystrings or wrong syntax

    my_debug_log("Missing POST parameter.");
    //    var_dump($_POST);
    //    $dump = ob_get_contents();
    //    ob_clean();
    //    my_debug_log($dump);

    echo "Unauthorized! 50101";
    http_response_code(403);
    exit(1);
}
else
{
    //querystring exists
    $username = $_POST['u'];
    $expires = $_POST['e'];
    $userhash = $_POST['h'];

    $app = $_POST['app'];
    $name = $_POST['name'];
    $addr = $_POST['addr'];

    //my_debug_log("u=$username&e=$expires&h=$userhash&app=$app&name=$name&addr=$addr");
}

// *****************************************
// Specify authorized users
if(!OpenConnection())
{
	echo "Connection error";
	my_debug_log("Connection error");
	http_response_code(501);
	exit(1);
}

$userdata = GetUserDataByUsername($username);
if(!$userdata || $userdata['active']==0)
{
	echo "User not found or inactive";
	my_debug_log("User not found or inactive");
	http_response_code(401);
	exit(1);
}

$pass = $userdata['password'];

// *****************************************

if ($pass == "")
{
    echo "Unauthenticated!";
    my_debug_log("Userdata password is empty");
    http_response_code(401);
    exit(1);
}

$input = "$username$expires$app$addr$name$pass";
//my_debug_log("Hashed string: $input");
//my_debug_log("Secret: $secret");

$myhash = hash_hmac("sha1", "$input", "$secret");

//my_debug_log("Hash: $myhash");

if (strtolower($userhash) != strtolower($myhash))
{
    echo "Unauthenticated!";
    my_debug_log("Hash mismatch");
    // my_debug_log("My hash: $myhash");
    http_response_code(401);
    exit(1);
}

// Check if hash is expired
if (time() > $expires)
{
    echo "Unauthenticated!";
    my_debug_log("Hash expired.");
    http_response_code(401);
    exit(1);
}

my_debug_log("$username authenticated");
echo "Greetings! ";

function my_debug_log($msg)
{
    $authlog = "/usr/local/nginx/html/auth.log";
    $dt = (new DateTime())->format("YmdHis");
    $myfile = fopen($authlog , "a") or die("Unable to open file: $filename");
    fwrite($myfile, $dt . ": " . $msg . PHP_EOL);
    fclose($myfile);
}
?>
