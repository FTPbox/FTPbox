<?php
class Security {

	public $config = array();
	public $absolutePath;
	public $relativePath;

	public function __construct() {
		session_start();
		$this->getConfig();
	}

	public function checkAuth($edit) {
		if($this->config['locked'] === 'false' || $this->checkSession()) {
			if(empty($this->config['root']) || !is_dir($this->config['root']) || $edit) {
				return 'config';
			} else {
				return 'browser';
			}
		} else {
			return 'login';
		}
	}

	public function checkLogin($password) {
		if(md5($password) === $this->config['password']) {
			$this->setSession($password);
			Header("Location: index.php");
		} else {
			return true;
		}
	}

	private function checkSession() {
		if(isset($_SESSION['sid']) && $_SESSION['sid'] == session_id() && isset($_SESSION['password']) && $_SESSION['password'] == $this->config['password']) {
			return true;
		}
	}

	private function getConfig() {
		$root = $_SERVER['DOCUMENT_ROOT'];
		require_once('system/config/config.php');
		$this->config = $config;
	}

	public function checkPath() {
		$path = '';
		if(isset($_REQUEST['p'])) {
			$path = $_REQUEST['p'];
		}
		if(substr($path, 0, 1) == DIRECTORY_SEPARATOR) {
			$path = substr($path, 1);
		}
		if(is_dir($this->config['root'].$path) && !strpos($path, '..'.DIRECTORY_SEPARATOR)) {
			$this->absolutePath = $this->config['root'].$path;
			chdir($this->absolutePath);
			$this->relativePath = $path;
		} else {
			die('No access!');
		}
	}

	public function setConfig($array) {
		if(!is_writable('system/config/config.php')) {
			return 'You are not allowed to change the config';
		}
		if(!empty($array['password']) && $array['locked'] == 'false') {
			return 'Please check "Protect the interface with a password" if you want to set a password';
		}
		if($array['password'] !== $array['password_repeat']) {
			return 'Please enter the same password';
		}
		if(!is_dir($array['root'])) {
			return 'Your root path doesn\'t exist';
		}
		unset($array['password_repeat']);
		$lines = file('system/config/config.php');
		foreach($array as $key => $value) {
			if($this->config[$key] != $value && !empty($value)) {
				if($key == 'root' && substr($value, -1) != DIRECTORY_SEPARATOR) {
					$value .= DIRECTORY_SEPARATOR;
				}
				if($key == 'password') {
					$value = md5($value);
				}
				$lines = $this->writeConfig($key, $value, $lines);
			}
		}
		$handle = fopen('system/config/config.php', 'w');
		fwrite($handle, implode($lines));
		fclose($handle);
		Header("Location: index.php");
	}

	private function writeConfig($key, $value, $lines) {
		foreach($lines as $linekey => $line) {
			if(strstr($line, '$config[\''.$key.'\']')) {
				$lines[$linekey] = '$config[\''.$key.'\'] = \''.$value.'\';'."\n";
				$this->config[$key] = $value;
			}
		}
		return $lines;
	}

	private function setSession($password) {
		$_SESSION['sid'] = session_id();
		$_SESSION['password'] = md5($password);
	}
}
?>
