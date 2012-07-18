<?php
require_once('system/classes/Filesystem.php');
require_once('system/classes/Gui.php');
require_once('system/classes/Security.php');
$security = new Security;
$gui = new Gui;
$filesystem = new Filesystem;
echo $gui->renderResults($filesystem->read($_GET['absolutePath'], $_GET['relativePath'], $_GET['sortName'], $_GET['sortOrder']));
?>
