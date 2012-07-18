<?php
if(isset($_GET['path'])) {
	require_once('system/classes/Upload.php');
	$allowedExtensions = array();
	$sizeLimit = 10 * 1024 * 1024;
	$uploader = new Upload($allowedExtensions, $sizeLimit);
	$result = $uploader->handleUpload($_GET['path'].'/');
	echo htmlspecialchars(json_encode($result), ENT_NOQUOTES);
}
?>
