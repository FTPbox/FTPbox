<?php
class Gui {

	private function formatfilesize($data) {
		if($data < 1024) {
			return $data." bytes";
		} elseif($data < 1024000) {
			return round(($data/1024),1)." kb";
		} else {
			return round(($data/1024000),1)." MB";
		} 
	}

	private function encodeString($string) {
		$string = htmlentities($string);
		$string = str_replace(' ', '&nsbp;', $string);
		$string = str_replace('.', '&point;', $string);
		return $string;
	}

	public function renderConfig($config, $status) {
		$array['status'] = $status;
		$array['locked'] = $config['locked'];
		$array['locked_label'] = 'Password-Protect the interface';
		if(empty($config['root'])) {
			$array['root'] = $this->filesystem->dirUp(getcwd());
		} else {
			$array['root'] = $config['root'];
		}
		$array['root_label'] = 'Rootpath';
		return $array;
	}

	public function renderLogin($retry = false) {
		if($retry) {
			$string = 'Please try again!';
		} else {
			$string = 'Please enter your password';
		}
		return $string;
	}

	public function renderResults($result) {
		$string = '';
		foreach ($result as $file) {
			$string .= '<div id="details-'.$file['filename'].'" class="row '.$file['mode'].'"><div class="details-checkbox">';
			if($file['mode'] != 'up') $string .= '<input type="checkbox" id="cbox-details-'.$file['filename'].'" name="'.$this->encodeString($file['filename']).'" />';
			
			if($file['mode'] == 'file') {
				if (substr($file['dirpath'], 0, 1) == '/')
					$string .= '</div><div class="details-link"><a href="..'.$file['dirpath'].'" target="_blank">'.$file['filename'].'</a></div><div class="details-permission">';
				else
					$string .= '</div><div class="details-link"><a href="../'.$file['dirpath'].'" target="_blank">'.$file['filename'].'</a></div><div class="details-permission">';				
			}
			else if ($file['mode'] == 'folder') $string .= '</div><div class="details-link"><a href="?p='.$file['dirpath'].'">'.$file['filename'].'</a></div><div class="details-permission">';
			
			if(isset($file['permission'])) $string .= $file['permission'];
			$string .= '</div><div class="details-size">';
			if(!empty($file['size'])) $string .= $this->formatfilesize($file['size']);
			$string .= '</div><div class="details-modtime">';
			if(isset($file['modtime'])) {
				if(time() - $file['modtime'] < 60 * 60 * 12) {
					$string .= date('H:i',$file['modtime']);
				} else {
					$string .= date('d.m.Y',$file['modtime']);
				}
			}
			$string .= '</div></div>';
		}
		return $string;
	}

	public function renderUp($dir, $root){
		if(strlen(getcwd()) > strlen($root)) {
			$string = '<a id="toolbox_buttons_up" class="button" href="index.php?p='.$dir.'" />Up</a>';
		} else {
			$string = '';
		}
		return $string;
	}
}
?>
