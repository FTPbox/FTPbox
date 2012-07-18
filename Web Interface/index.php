<?php
require_once('system/classes/Filesystem.php');
require_once('system/classes/Gui.php');
require_once('system/classes/Security.php');
require_once('system/savant/Savant3.php');
$conf = array(
	'template_path' => dirname($_SERVER['SCRIPT_FILENAME']).'/layout/templates'
);
$tpl = new Savant3($conf);
$security = new Security;
$gui = new Gui;
$filesystem = new Filesystem;
$gui->filesystem = $filesystem;
switch($security->checkAuth(isset($_REQUEST['config']))) {
	case 'config':
		if(isset($_POST['root'])) {
			$status = $security->setConfig(array('root' => $_POST['root'], 'locked' => (isset($_POST['locked']))?'true':'false', 'password' => $_POST['password'], 'password_repeat' => $_POST['password_repeat']));
		} else {
			$status = '';
		}
		$tpl->content = $gui->renderConfig($security->config, $status);
		$content = 'config.tpl.php';
		break;
	case 'login':
		if(isset($_POST['password'])) $retry = $security->checkLogin($_POST['password']);
		$tpl->content = $gui->renderLogin(isset($retry));
		$content = 'login.tpl.php';
		break;
	case 'browser':
		$security->checkPath();
		if(isset($_POST['submit_delete'])) {
			unset($_POST['submit_delete'], $_POST['p']);
			$filesystem->delete($security->absolutePath, $_POST);
		}
		$tpl->absolutePath = $security->absolutePath;
		$tpl->relativePath = $security->relativePath;
		$tpl->sortName = (isset($_GET['sortName'])?$_GET['sortName']:'filename');
		$tpl->sortOrder = (isset($_GET['sortOrder'])?$_GET['sortOrder']:'asc');
		$tpl->content = $gui->renderResults($filesystem->read($security->absolutePath, $security->relativePath, $tpl->sortName, $tpl->sortOrder));
		$tpl->up = $gui->renderUp($filesystem->dirUp($security->relativePath), $security->config['root']);
		$content = 'browser.tpl.php';
		break;
}
$tpl->display('header.tpl.php');
$tpl->display($content);
$tpl->display('footer.tpl.php');
?>