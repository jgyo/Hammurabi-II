<?php

// MySql Database for accounts
$dbName = 'accounts';

// MySql account and password
$db_username = "mySqlUsername";
$db_password = "mySqlPassword";

$db_conn = null;
$sql_error = "";

$ualpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
$lalpha = "abcedfghijklmnopqrstuvwxyz";
$numbs = "0123456789";
$symbs = "!#$%()*+,-.:;<=>?[]^_{}~";
$valid_password_characters = $ualpha . $lalpha . $numbs . $symbs;

function OpenConnection()
{
	global $db_password, $db_conn, $dbName, $db_username;

	$db_conn = mysqli_connect("localhost", $db_username, $db_password, $dbName);
	return $db_conn == true;
}


function CloseConnection()
{
	global $db_conn;

	if ($db_conn)
		$db_conn->close();

	$db_conn = null;
}

function UpdateUserPassword($id, $newPassword) {
	global $db_conn, $sql_error;

	if ( ! $db_conn ) {
		$sql_error = "SQL connection is not active.";
		return false;
	}

	$sql = "UPDATE accounts SET password=$newPassword WHERE id=$id";
	if ( mysqli_query( $db_conn, $sql ) ) {
		return true;
	}

	$sql_error = mysqli_error($db_conn);
	return false;
}

function CountChars($chars, $target)
{
	$c = 0;
	for($i=0; $i<strlen($chars); $i++)
	{
		for($j=0; $j<strlen($target); $j++)
		{
			if($chars[$i] == $target[$j])
			{
				$c++;
			}
		}
	}

	return $c;
}

function validatePassword ($password, $len=8, $uc=1, $lc=1, $nc=1, $sc=1)
{
	global $ualpha, $lalpha, $numbs, $symbs;

	if(strlen($password) < $len)
	{
		return false;
	}

	if($uc && CountChars($ualpha, $password) < $uc)
	{
		return false;
	}
	if($lc && CountChars($lalpha, $password) < $lc)
	{
		return false;
	}
	if($nc && CountChars($numbs, $password) < $nc)
	{
		return false;
	}
	if($sc && CountChars($symbs, $password) < $sc)
	{
		return false;
	}
	return true;
}


function UpdateUserData($id, $username, $email, $password, $active)
{
	global $db_conn, $sql_error;

	if ( ! $db_conn ) {
		$sql_error = "SQL connection is not active.";
		return false;
	}

	$username = strtolower(mysqli_escape_string($db_conn, $username));
	$email = strtolower(mysqli_escape_string($db_conn, $email));
	$password = mysqli_escape_string($db_conn, $password);

	if(!$id || !strlen($username) || !strlen($email) || !strlen($password))
	{
		$sql_error = "Invalid data received. All fields are required.";
		return false;
	}

	$sql = "UPDATE accounts SET active=$active, username='$username', password='$password', email='$email' WHERE id=$id;";
	if ( mysqli_query( $db_conn, $sql ) ) {
		return true;
	}

	$sql_error = mysqli_error($db_conn);
	return false;
}

function GetUserDataById($id) {
	global $db_conn, $sql_error;

	if ( ! $db_conn ) {
		$sql_error = "SQL connection is not active.";
		return false;
	}

	$sql    = "SELECT * FROM accounts WHERE id = $id";
	$result = mysqli_query( $db_conn, $sql );
	if ( ! $result ) {
		$sql_error = mysqli_error($db_conn);
		return false;
	}

	$row = $result->fetch_assoc();
	$result->free();
	return $row;
}

function GetUserDataByEmail($email)
{
	global $db_conn, $sql_error;

	if ( ! $db_conn ) {
		$sql_error = "SQL connection is not active.";
		return false;
	}

	$email = strtolower(mysqli_escape_string( $db_conn, $email ));
	$sql            = "SELECT * FROM accounts WHERE email = '$email'";
	$result         = mysqli_query( $db_conn, $sql );
	if ( ! $result ) {
		$sql_error = mysqli_error($db_conn);
		return false;
	}

	$row = $result->fetch_assoc();
	$result->free();
	return $row;
}

function GetUserDataByUsername($media_username) {
	global $db_conn, $sql_error;

	if ( ! $db_conn ) {
		$sql_error = "SQL connection is not active.";
		return false;
	}

	$media_username = strtolower(mysqli_escape_string( $db_conn, $media_username ));

	$sql            = "SELECT * FROM accounts WHERE username = '$media_username'";
	$result         = mysqli_query( $db_conn, $sql );
	if ( ! $result ) {
		$sql_error = mysqli_error($db_conn);
		return false;
	}

	$row = $result->fetch_assoc();
	$result->free();
	return $row;
}

function GetUserAccounts() {
	global $db_conn, $sql_error;

	if ( ! $db_conn ) {
		$sql_error = "SQL connection is not active.";
		return false;
	}

	$sql   = "SELECT * FROM accounts ORDER BY username";
	$query = mysqli_query( $db_conn, $sql );
	if ( ! $query ) {
		$sql_error = mysqli_error($db_conn);
		return false;
	}

	while($row = mysqli_fetch_row($query))
	{
		$accounts[] = array(
			"id" => $row[0],
			"username" => $row[1],
			"email" => $row[2],
			"password" => $row[3],
			"active" => $row[4],
			"created" => $row[5],
			"modified" => $row[6]
		);
	}

	$query->free();
	return $accounts;
}

function IsActive($id) {
	global $db_conn, $sql_error;

	if ( ! $db_conn ) {
		$sql_error = mysqli_error($db_conn);
		return false;
	}

	$sql = "SELECT active FROM accounts WHERE id = '$id'";
    $result = mysqli_query($db_conn, $sql);
    if(!$result)
        return false;

    $row = $result->fetch_assoc();
    $result->free();
    return $row['active'] == 1;
}

function IsUsernameUnique($media_username) {
	global $db_conn, $sql_error;

	if ( ! $db_conn ) {
		$sql_error = "SQL connection is not active.";
		return false;
	}

	if ( GetUserDataByUsername( $media_username ) ) {
		$sql_error = mysqli_error($db_conn);
		return false;
	}

	return true;
}

function CreateNewTitle($filename, $speaker, $date, $time) {
	global $title_conn, $sql_error;

	if ( ! $title_conn ) {
		$sql_error = "SQL connection is not active.";
		return false;
	}

	$filename = mysqli_real_escape_string($title_conn, $filename);
	$speaker= mysqli_real_escape_string($title_conn, $speaker);

	if(!IsFilenameUnique($filename))
	{
		$sql_error = "Filename \"$filename\" is not unique.";
		return false;
	}



	$sql = "INSERT INTO media (filename, speaker, date, time, views) VALUES ('$filename', '$speaker', $date, $time, 0)";
	if ($title_conn->query($sql))
	{
		$titledata = GetTitleDataByFilename($filename);
		return $titledata ['id'];
	}

	$sql_error = mysqli_error($title_conn);
	return false;
}

function CreateNewAccount($username, $email, $password, $active) {
	global $db_conn, $sql_error;

	if ( ! $db_conn ) {
		$sql_error = "SQL connection is not active.";
		return false;
	}

	$username = strtolower(mysqli_real_escape_string($db_conn, $username));

	if(!IsUsernameUnique($username))
	{
		$sql_error = "Username \"$username\" is not unique.";
		return false;
	}

	$password = mysqli_real_escape_string($db_conn, $password);
	$email = mysqli_real_escape_string($db_conn, $email);

	$sql = "INSERT INTO accounts (username, email, password, active) VALUES ('$username', '$email', '$password', $active)";
	if ($db_conn->query($sql))
	{
		$userdata = GetUserDataByUsername($username);
		return $userdata['id'];
	}

	$sql_error = mysqli_error($db_conn);
	return false;
}

function EnableAccount($id) {
	global $db_conn, $sql_error;
	if ( ! $db_conn ) {
		$sql_error = "SQL connection is not active.";
		return false;
	}

	$sql = "UPDATE accounts SET active=1 WHERE id=$id";
	if ( mysqli_query( $db_conn, $sql ) ) {
		return true;
	}

	$sql_error = mysqli_error( $db_conn );
	return false;
}

function DisableAccount($id) {
	global $db_conn, $sql_error;

	if ( ! $db_conn ) {
		$sql_error = "SQL connection is not active.";
		return false;
	}

	$sql = "UPDATE accounts SET active=0 WHERE id=$id";
	if ( mysqli_query( $db_conn, $sql ) ) {
		return true;
	}

	$sql_error = mysqli_error( $db_conn );
	return false;
}

function DeleteAccount($id)
{
	global $db_conn, $sql_error;

	if ( ! $db_conn ) {
		$sql_error = "SQL connection is not active.";
		return false;
	}

	$sql = "DELETE FROM accounts WHERE id=$id";
	if ( mysqli_query( $db_conn, $sql ) ) {
		return true;
	}

	$sql_error = mysqli_error( $db_conn );
	return false;
}


function GetNonce()
{
    $nonce = hash( 'sha512', makeRandomString() );
    $_SESSION['nonce'] = $nonce;
    return $nonce;
}

function VerifyNonce($cnonce)
{
	$nonce = $_SESSION['nonce']; // Fetch the nonce from the last request
	unset($_SESSION['nonce']);

    return $nonce == $cnonce;
}

function IsAdministrator()
{
	$id = $_SESSION['SESS_MEMBER_ID'];
	return $id && $id <= 3;
}

function IsLoggedIn()
{
	return $_SESSION['SESS_MEMBER_ID'];
}

function LogOut()
{
	session_unset();
	session_destroy();
	header('location:/login.php');
}

function makeRandomString($bits = 256) {
	$bytes = ceil($bits / 8);
	$return = '';
	for ($i = 0; $i < $bytes; $i++) {
		$return .= chr(mt_rand(0, 255));
	}

	return $return;
}

function MakePassword($len=8, $uc=1, $lc=1, $nc=1, $sc=1)
{
    global $ualpha, $lalpha, $numbs, $symbs;

    $chars = "";
    $pw = "";
    $pw .= GetRndChars($uc, $ualpha);
    $pw .= GetRndChars($lc, $lalpha);
    $pw .= GetRndChars($nc, $numbs);
    $pw .= GetRndChars($sc, $symbs);

    if(strlen($pw) < $len)
    {
        if($uc >= 0)
        {
            $chars .= $ualpha;
        }
        if($lc >= 0 )
        {
            $chars .= $lalpha;
        }
        if($nc >= 0)
        {
            $chars .= $numbs;
        }
        if($sc >= 1)
        {
            $chars .= $symbs;
        }
        while (strlen($pw) < $len)
        {
            $pw .= GetRndChar($chars);
        }
    }

    return str_shuffle($pw);

}

function GetRndChars($cnt, $chars) {
	$rndchars = "";
	for ( $i = 0; $i < $cnt; $i ++ ) {
		$rndchars .= GetRndChar( $chars );
	}
	return $rndchars;
}

function GetRndChar($chars)
{
	$hi = strlen($chars) - 1;
	$rnd = mt_rand(0, $hi);

	return $chars[$rnd];
}

?>